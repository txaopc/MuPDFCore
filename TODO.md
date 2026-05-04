# Digital Signature Feature Implementation

## Steps

- [x] 1. Add `SignatureField` model class and `SignatureFieldClickedEventArgs` to `MuPDFCore.MuPDFRenderer`
- [x] 2. Add signature-related properties and events to `PDFRenderer.Properties.cs` (`SignatureFields`, `SignatureFieldBrush`, `SignatureFieldHoverBrush`, `SignatureFieldClicked` event)
- [x] 3. Update `PDFRenderer.cs` to render signature field overlays, handle hover highlight, and detect clicks on signature fields
- [x] 4. Add `itext7` 7.2.3 NuGet reference to `PDFViewerDemo.csproj`
- [x] 5. Create `PDFViewerDemo/SignatureHelper.cs` — iText-based signature extraction
- [x] 6. Create `PDFViewerDemo/SignatureInfoWindow.axaml` and `.axaml.cs` — dialog to display signature info
- [x] 7. Update `PDFViewerDemo/MainWindow.axaml.cs` — wire up signature detection, store file path, handle click events
- [x] 8. Build and verify compilation ✅ (0 errors, 0 warnings)
