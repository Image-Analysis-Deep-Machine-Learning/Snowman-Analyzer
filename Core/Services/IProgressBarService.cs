namespace Snowman.Core.Services;

public interface IProgressBarService : IService
{
    void StartProgress(string jobDescription);
    void SetProgress(int percentage);
    void FinishProgress(string finishMessage);
}
