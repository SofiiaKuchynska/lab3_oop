namespace lab3.Models;

public class SearchCriteria
{
    public string AttributeName { get; set; } = string.Empty;
    public string SearchValue { get; set; } = string.Empty;

    public SearchCriteria(string attributeName, string searchValue)
    {
        AttributeName = attributeName;
        SearchValue = searchValue;
    }

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(AttributeName) || string.IsNullOrWhiteSpace(SearchValue);
    }
}