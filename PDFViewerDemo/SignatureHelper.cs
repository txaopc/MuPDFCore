/*
    PDFViewerDemo - A demo application for MuPDFCore.MuPDFRenderer.
    Uses iText 7.2.3 to extract digital signature information from PDF documents.
*/

using Avalonia;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Signatures;
using MuPDFCore.MuPDFRenderer;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDFViewerDemo
{
    /// <summary>
    /// Helper class to extract digital signature field information from a PDF using iText 7.2.3.
    /// </summary>
    public static class SignatureHelper
    {
        /// <summary>
        /// Extracts all digital signature fields from the specified PDF file.
        /// </summary>
        /// <param name="pdfFilePath">The path to the PDF file.</param>
        /// <returns>A list of <see cref="SignatureField"/> objects with metadata populated.</returns>
        public static List<SignatureField> ExtractSignatureFields(string pdfFilePath)
        {
            var result = new List<SignatureField>();

            try
            {
                using (PdfReader reader = new PdfReader(pdfFilePath))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    SignatureUtil signatureUtil = new SignatureUtil(pdfDoc);
                    PdfAcroForm acroForm = PdfAcroForm.GetAcroForm(pdfDoc, false);

                    if (acroForm == null)
                        return result;

                    IList<string> signatureNames = signatureUtil.GetSignatureNames();

                    foreach (string sigName in signatureNames)
                    {
                        try
                        {
                            SignatureField field = ExtractSingleSignature(pdfDoc, acroForm, signatureUtil, sigName);
                            if (field != null)
                            {
                                result.Add(field);
                            }
                        }
                        catch (Exception ex)
                        {
                            // If one signature fails, continue with others
                            System.Diagnostics.Debug.WriteLine($"Failed to extract signature '{sigName}': {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to extract signatures from PDF: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Extracts a single signature field's metadata.
        /// </summary>
        private static SignatureField ExtractSingleSignature(PdfDocument pdfDoc, PdfAcroForm acroForm, SignatureUtil signatureUtil, string sigName)
        {
            PdfFormField formField = acroForm.GetField(sigName);
            if (formField == null)
                return null;

            // Determine page number and bounds
            int pageNumber = 0;
            Rect bounds = default;

            PdfWidgetAnnotation widget = formField.GetWidgets()?.FirstOrDefault();
            if (widget != null)
            {
                PdfPage page = widget.GetPage();
                if (page != null)
                {
                    pageNumber = pdfDoc.GetPageNumber(page) - 1; // Convert to 0-based
                }
                else
                {
                    // Fallback: search all pages for this widget
                    pageNumber = FindPageForWidget(pdfDoc, widget);
                }

                iText.Kernel.Geom.Rectangle rect = widget.GetRectangle()?.ToRectangle();
                if (rect != null)
                {
                    // Get page height for coordinate conversion (PDF origin is bottom-left, MuPDF is top-left)
                    float pageHeight = 0;
                    PdfPage sigPage = pdfDoc.GetPage(pageNumber + 1); // back to 1-based for iText
                    if (sigPage != null)
                    {
                        iText.Kernel.Geom.Rectangle mediaBox = sigPage.GetMediaBox();
                        pageHeight = mediaBox.GetHeight();
                    }

                    // Convert from PDF coordinates (bottom-left origin) to top-left origin
                    double left = rect.GetX();
                    double bottom = rect.GetY();
                    double width = rect.GetWidth();
                    double height = rect.GetHeight();
                    double top = pageHeight - bottom - height;

                    bounds = new Rect(left, top, width, height);
                }
            }

            SignatureField sigField = new SignatureField(sigName, pageNumber, bounds);

            // Extract signature dictionary info
            PdfSignature pkcs7 = signatureUtil.GetSignature(sigName);
            if (pkcs7 != null)
            {
                sigField.SignerName = pkcs7.GetName();
                sigField.Reason = pkcs7.GetReason();
                sigField.Location = pkcs7.GetLocation();
                // ContactInfo is not directly available as a method in iText 7.2.3;
                // retrieve it from the underlying PDF dictionary.
                PdfString contactInfoStr = pkcs7.GetPdfObject()?.GetAsString(PdfName.ContactInfo);
                sigField.ContactInfo = contactInfoStr?.GetValue();

                PdfString dateStr = pkcs7.GetDate();
                if (dateStr != null)
                {
                    try
                    {
                        sigField.SigningTime = PdfDate.Decode(dateStr.GetValue()).ToUniversalTime();
                    }
                    catch
                    {
                        // Date parsing failed, leave as null
                    }
                }
            }

            // Extract certificate information using PdfPKCS7
            try
            {
                PdfPKCS7 pkcs7Verifier = signatureUtil.ReadSignatureData(sigName);
                if (pkcs7Verifier != null)
                {
                    X509Certificate signingCert = pkcs7Verifier.GetSigningCertificate();
                    if (signingCert != null)
                    {
                        sigField.CertificateSubject = signingCert.SubjectDN?.ToString();
                        sigField.CertificateIssuer = signingCert.IssuerDN?.ToString();
                        sigField.CertificateValidFrom = signingCert.NotBefore;
                        sigField.CertificateValidTo = signingCert.NotAfter;
                        sigField.CertificateSerialNumber = signingCert.SerialNumber?.ToString();
                    }

                    // Signer name fallback from certificate if not in signature dict
                    if (string.IsNullOrEmpty(sigField.SignerName))
                    {
                        sigField.SignerName = ExtractCommonName(signingCert?.SubjectDN?.ToString());
                    }

                    sigField.SigningTime = sigField.SigningTime ?? pkcs7Verifier.GetSignDate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read PKCS7 data for '{sigName}': {ex.Message}");
            }

            // Check if signature covers whole document
            try
            {
                sigField.CoversWholeDocument = signatureUtil.SignatureCoversWholeDocument(sigName);
            }
            catch
            {
                sigField.CoversWholeDocument = false;
            }

            return sigField;
        }

        /// <summary>
        /// Searches all pages to find which page contains the given widget annotation.
        /// </summary>
        private static int FindPageForWidget(PdfDocument pdfDoc, PdfWidgetAnnotation widget)
        {
            PdfObject widgetObj = widget.GetPdfObject();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDoc.GetPage(i);
                IList<PdfAnnotation> annotations = page.GetAnnotations();
                foreach (PdfAnnotation annot in annotations)
                {
                    if (annot.GetPdfObject() == widgetObj)
                    {
                        return i - 1; // 0-based
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Extracts the Common Name (CN) from a distinguished name string.
        /// </summary>
        /// <param name="distinguishedName">The full DN string (e.g., "CN=John Doe, O=Acme Corp").</param>
        /// <returns>The CN value, or the full string if CN is not found.</returns>
        private static string ExtractCommonName(string distinguishedName)
        {
            if (string.IsNullOrEmpty(distinguishedName))
                return null;

            // Try to extract CN=... from the DN
            foreach (string part in distinguishedName.Split(','))
            {
                string trimmed = part.Trim();
                if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(3).Trim();
                }
            }

            return distinguishedName;
        }
    }
}
