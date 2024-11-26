


using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticKernel.Embeddings;

var builder = WebApplication.CreateBuilder(args);

// Create a service collection
var serviceProvider = new ServiceCollection()
               .AddSingleton<IPdfReaderService, PdfReaderService>()// Register PdfReaderService
                .BuildServiceProvider();

// Get the PdfReaderService from the DI container
var pdfReaderService = serviceProvider.GetService<IPdfReaderService>();

var filePath = "";

// Use the PdfReaderService to read the PDF
await pdfReaderService.ReadPdf(filePath);

Console.ReadLine();
