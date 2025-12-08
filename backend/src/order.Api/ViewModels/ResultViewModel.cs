namespace Order.Api.ViewModels;

public class ResultViewModel<T>
{
    private readonly List<string> _errors = new();

    public ResultViewModel() { }

    public ResultViewModel(T data)
    {
        Data = data;
    }

    public ResultViewModel(T data, IEnumerable<string> errors)
    {
        Data = data;
        _errors.AddRange(errors);
    }

    public ResultViewModel(IEnumerable<string> errors)
    {
        _errors.AddRange(errors);
    }

    public ResultViewModel(string error)
    {
        _errors.Add(error);
    }

    public T? Data { get; set; }

    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string error)
    {
        _errors.Add(error);
    }
}
