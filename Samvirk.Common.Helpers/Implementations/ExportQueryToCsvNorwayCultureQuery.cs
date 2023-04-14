using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Fabillio.Common.Helpers.GlobalHelpers;
using Fabillio.Common.Helpers.Implementations.Converters;

namespace Fabillio.Common.Helpers.Implementations;

public static class ExportQueryToCsvNorwayCultureQuery
{
    private const string _csvTextDelimiter = ";";

    public static byte[] CreateCSVEncodedResults<TToExport, TMapper>(
        List<TToExport> dataToExport,
        Action<CustomConverterOptions> action
    ) where TMapper : ClassMap
    {
        var options = new CustomConverterOptions();
        action(options);

        var configurations = new CsvConfiguration(CultureHelper.GetCustomCultureInfo())
        {
            Delimiter = _csvTextDelimiter,
            IgnoreBlankLines = false
        };

        using var fileStream = new MemoryStream();
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        using (var csvWriter = new CsvWriter(streamWriter, configurations))
        {
            if (options.Decimal)
            {
                csvWriter.Context.TypeConverterCache.AddConverter<decimal>(
                    new CustomDecimalConverter()
                );
            }

            if (options.DateTime)
            {
                csvWriter.Context.TypeConverterCache.AddConverter<DateTime>(
                    new CustomDateTimeConverter()
                );
            }

            csvWriter.Context.RegisterClassMap<TMapper>();
            csvWriter.WriteHeader<TToExport>();
            csvWriter.NextRecord();

            foreach (var model in dataToExport)
            {
                csvWriter.WriteRecord(model);
                csvWriter.NextRecord();
            }
        }

        return fileStream.ToArray();
    }

    public static async Task<List<TExport>> MapFromFormFile<TExport>(IFormFile formFile)
    {
        List<TExport> exports = new();
        using (var streamReader = new StreamReader(formFile.OpenReadStream()))
        {
            using var csvReader = new CsvReader(
                streamReader,
                new CsvConfiguration(CultureHelper.GetCustomCultureInfo())
                {
                    Delimiter = _csvTextDelimiter,
                    IgnoreBlankLines = false,
                }
            );

            var records = csvReader.GetRecordsAsync<TExport>();

            await foreach (var item in records)
            {
                exports.Add(item);
            }
        }

        return exports;
    }
}
