/*
    MuPDFCore.MuPDFRenderer - A control to display documents in Avalonia using MuPDFCore.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using Avalonia;
using System;
using System.Collections.Generic;

namespace MuPDFCore.MuPDFRenderer
{
    /// <summary>
    /// Represents a digital signature field in a PDF document, including its visual position and metadata.
    /// </summary>
    public class SignatureField
    {
        /// <summary>
        /// The name of the signature field in the PDF form.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The page number (0-based) on which this signature field appears.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The bounding rectangle of the signature field in PDF page coordinates (origin at top-left, in page units).
        /// This should already be converted from PDF coordinates (origin bottom-left) to MuPDF coordinates (origin top-left).
        /// </summary>
        public Rect Bounds { get; set; }

        /// <summary>
        /// The name of the signer.
        /// </summary>
        public string SignerName { get; set; }

        /// <summary>
        /// The date and time when the signature was applied.
        /// </summary>
        public DateTime? SigningTime { get; set; }

        /// <summary>
        /// The reason for signing, if provided.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The location where the signature was applied, if provided.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The contact info of the signer, if provided.
        /// </summary>
        public string ContactInfo { get; set; }

        /// <summary>
        /// The subject of the signing certificate.
        /// </summary>
        public string CertificateSubject { get; set; }

        /// <summary>
        /// The issuer of the signing certificate.
        /// </summary>
        public string CertificateIssuer { get; set; }

        /// <summary>
        /// The start of the certificate validity period.
        /// </summary>
        public DateTime? CertificateValidFrom { get; set; }

        /// <summary>
        /// The end of the certificate validity period.
        /// </summary>
        public DateTime? CertificateValidTo { get; set; }

        /// <summary>
        /// The serial number of the signing certificate.
        /// </summary>
        public string CertificateSerialNumber { get; set; }

        /// <summary>
        /// Whether the signature covers the entire document (i.e., no modifications after signing).
        /// </summary>
        public bool CoversWholeDocument { get; set; }

        /// <summary>
        /// Additional metadata associated with this signature field.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new <see cref="SignatureField"/> instance.
        /// </summary>
        public SignatureField()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SignatureField"/> instance with the specified parameters.
        /// </summary>
        /// <param name="fieldName">The name of the signature field.</param>
        /// <param name="pageNumber">The 0-based page number.</param>
        /// <param name="bounds">The bounding rectangle in page coordinates (top-left origin).</param>
        public SignatureField(string fieldName, int pageNumber, Rect bounds)
        {
            FieldName = fieldName;
            PageNumber = pageNumber;
            Bounds = bounds;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="PDFRenderer.SignatureFieldClicked"/> event.
    /// </summary>
    public class SignatureFieldClickedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="SignatureField"/> that was clicked.
        /// </summary>
        public SignatureField SignatureField { get; }

        /// <summary>
        /// Creates a new <see cref="SignatureFieldClickedEventArgs"/> instance.
        /// </summary>
        /// <param name="signatureField">The signature field that was clicked.</param>
        public SignatureFieldClickedEventArgs(SignatureField signatureField)
        {
            SignatureField = signatureField;
        }
    }
}
