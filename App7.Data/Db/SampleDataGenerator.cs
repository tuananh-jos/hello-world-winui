using System.Text.Json;
using App7.Domain.Entities;
using Bogus;

namespace App7.Data.Db;

public static class SampleDataGenerator
{
    public static async Task GenerateAsync()
    {
        const int modelCount = 1000000;
        const int devicesPerModel = 3;

        var modelFaker = new Faker<Model>()
            .RuleFor(m => m.Id, f => Guid.NewGuid())
            .RuleFor(m => m.Name, f => $"X{f.Random.Number(100, 999)}G")
            .RuleFor(m => m.Manufacturer, f => f.PickRandom("Samsung", "Apple", "Xiaomi", "Oppo"))
            .RuleFor(m => m.Category, f => "Smartphone")
            .RuleFor(m => m.SubCategory, f => f.PickRandom("Galaxy A", "Galaxy S", "Note"))
            .RuleFor(m => m.Available, _ => 0); // will be computed after devices are generated

        var models = modelFaker.Generate(modelCount);

        var deviceFaker = new Faker<Device>()
            .RuleFor(d => d.Id, f => Guid.NewGuid())
            .RuleFor(d => d.ModelId, f => f.PickRandom(models).Id)
            .RuleFor(d => d.Name, f => $"SM-{f.Random.AlphaNumeric(6).ToUpper()}")
            .RuleFor(d => d.IMEI, f => f.Random.ReplaceNumbers("###############"))
            .RuleFor(d => d.SerialLab, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(d => d.SerialNumber, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(d => d.CircuitSerialNumber, f => f.Random.AlphaNumeric(11).ToUpper())
            .RuleFor(d => d.HWVersion, f => $"REV0{f.Random.Number(1, 5)}")
            .RuleFor(d => d.Status, _ => "Available");

        var devices = deviceFaker.Generate(modelCount * devicesPerModel);

        // Compute Available count per model from actual generated devices
        var availableByModel = devices
            .Where(d => d.Status == "Available")
            .GroupBy(d => d.ModelId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var model in models)
        {
            model.Available = availableByModel.TryGetValue(model.Id, out var count)
                ? count
                : 0;
        }

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
            WriteIndented = true,
            // Cấu hình này giúp xử lý các object lớn mượt mà hơn
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // 1. Mở FileStream để ghi trực tiếp xuống ổ cứng
        using FileStream createStream = File.Create(path);

        // 2. Sử dụng SerializeAsync để đẩy dữ liệu từ RAM xuống Stream theo từng mảng nhỏ (buffer)
        await JsonSerializer.SerializeAsync(createStream, data, options);

        // 3. Đảm bảo dữ liệu được đẩy xuống đĩa hoàn toàn
        await createStream.FlushAsync();
    }
}