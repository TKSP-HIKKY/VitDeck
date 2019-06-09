namespace VitDeck.Validator
{
    /// <summary>
    /// 検証ルールが持つべきインターフェース
    /// </summary>
    public interface IRule
    {
        ValidationResult Validate(string baseFolder);
        ValidationResult GetResult();
    }
}