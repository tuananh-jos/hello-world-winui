using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using DeviceManagement.AI.Tools;

namespace DeviceManagement.AI.Services
{
    public class AIService
    {
        private readonly Kernel _kernel;

        public AIService()
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini", // hoặc model bạn dùng
                apiKey: "YOUR_OPENAI_KEY"
            );

            _kernel = builder.Build();

            // 👇 Đây là chỗ register tool
            var projectRoot = @"C:\Users\Tai Khoan\Downloads\hello-world-winui-main\hello-world-winui-main";
            var fileTools = new FileTools(projectRoot);

            _kernel.ImportPluginFromObject(fileTools, "FileTools");
        }

        public async Task<string> AskAsync(string prompt)
        {
            var result = await _kernel.InvokePromptAsync(prompt);
            return result.ToString();
        }
    }
}
