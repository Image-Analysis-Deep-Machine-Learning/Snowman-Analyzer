using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Snowman.Core.MachineLearning;

namespace Snowman.Core.Services;

public interface IChatService : IService
{
    public void OpenChatWindow();
    public void SetChatHistory(ObservableCollection<Chat> chatHistory);
    public void DeleteChat(Chat chat);
    public void CloseChatWindow();
    public void SelectChat(Chat chat);
    public void StopGeneratingResponse();
    public Task<bool> SendUserPrompt(string prompt);
}
