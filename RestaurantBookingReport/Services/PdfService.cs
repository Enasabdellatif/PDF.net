using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using RestaurantBookingReport.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas.Draw;

namespace RestaurantBookingReport.Services
{
    public class PdfService
    {
        public MemoryStream GeneratePdf(RestaurantReport report,DateTime Date)
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Add title
            document.Add(new Paragraph(report.RestaurantTitle)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Report Date: {Date.ToString("MM/dd/yyyy")}").SetTextAlignment(TextAlignment.LEFT));

            document.Add(new LineSeparator(new SolidLine()));

            // Add total reservations and pax
            document.Add(new Paragraph($"Total Reservations: {report.TotalReservations}"));
            document.Add(new Paragraph($"Total Pax: {report.TotalPax}"));

            document.Add(new LineSeparator(new SolidLine()));

            // Add seat data
            foreach (var seat in report.SeatData)
            {
                document.Add(new Paragraph($"Seat Time: {seat.Time}"));
                document.Add(new Paragraph($"Reservations: {seat.TotalReservations}"));
                document.Add(new Paragraph($"People: {seat.TotalPeople}"));

                document.Add(new LineSeparator(new SolidLine()));

                // Add table with booking details
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 15, 10, 10, 15, 10, 15, 10 })).UseAllAvailableWidth();
                table.AddHeaderCell("Created");
                table.AddHeaderCell("Last Name");
                table.AddHeaderCell("Room");
                table.AddHeaderCell("Pax");
                table.AddHeaderCell("Events");
                table.AddHeaderCell("VIP");
                table.AddHeaderCell("Notes");
                table.AddHeaderCell("Kids");

                foreach (var booking in seat.Bookings)
                {
                    table.AddCell(new Cell().Add(new Paragraph(booking.Created)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.LastName)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.Room)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.People.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(booking.Events)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.VIP)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.Notes)));
                    table.AddCell(new Cell().Add(new Paragraph(booking.Kids.ToString())));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return stream;
        }
    }
}
