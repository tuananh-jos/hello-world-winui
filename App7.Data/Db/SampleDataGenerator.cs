using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using App7.Domain.Entities;
using Bogus;

namespace App7.Data.Db;
public static class SampleDataGenerator
{
    public static async Task GenerateAsync()
    {
        const int modelCount = 50;
        const int devicesPerModel = 20;

        var modelFaker = new Faker<Model>()
            .RuleFor(m => m.Id, f => Guid.NewGuid())
            .RuleFor(m => m.Name, f => $"X{f.Random.Number(100,999)}G")
            .RuleFor(m => m.Manufacturer, f => f.PickRandom("Samsung", "Apple", "Xiaomi", "Oppo"))
            .RuleFor(m => m.Category, f => "Smartphone")
            .RuleFor(m => m.SubCategory, f => f.PickRandom("Galaxy A", "Galaxy S", "Note"))
            .RuleFor(m => m.Available, f => f.Random.Number(0, 1));

        var models = modelFaker.Generate(modelCount);

        var deviceFaker = new Faker<Device>()
            .RuleFor(d => d.Id, f => Guid.NewGuid())
            .RuleFor(d => d.ModelId, f => f.PickRandom(models).Id)
            .RuleFor(d => d.Name, f => $"SM-{f.Random.AlphaNumeric(6).ToUpper()}")
            .RuleFor(d => d.IMEI, f => f.Random.ReplaceNumbers("###############"))
            .RuleFor(d => d.SerialLab, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(d => d.SerialNumber, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(d => d.CircuitSerialNumber, f => f.Random.AlphaNumeric(11).ToUpper())
            .RuleFor(d => d.HWVersion, f => $"REV0{f.Random.Number(1,5)}");

        var devices = deviceFaker.Generate(modelCount * devicesPerModel);

        await WriteJsonAsync("models.json", models);
        await WriteJsonAsync("devices.json", devices);
    }

    private static async Task WriteJsonAsync<T>(string fileName, T data)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data",
            fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(path, json);
    }
}