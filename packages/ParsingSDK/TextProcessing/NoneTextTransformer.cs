namespace ParsingSDK.TextProcessing;

public sealed class NoneTextTransformer : ITextTransformer
{
    public string TransformText(string text)
    {
        return text;
    }
}