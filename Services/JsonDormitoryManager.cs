using System.Text.Encodings.Web;
using System.Text.Json;
using lab3.Models;

namespace lab3.Services;

public class JsonDormitoryManager
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    private List<Student> _allStudents = new();

    public List<Student> GetAll() => new(_allStudents);

    public async Task<List<Student>?> OpenJsonFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть JSON-файл гуртожитку",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.Android, new[] { "application/json" } }
                })
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                var loaded = await JsonSerializer.DeserializeAsync<List<Student>>(stream, _jsonOptions);
                _allStudents = loaded ?? new List<Student>();
                return new List<Student>(_allStudents);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Open error: {ex.Message}");
        }

        return null;
    }

    public List<Student> LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath)) return new List<Student>();
        var json = File.ReadAllText(filePath);
        _allStudents = JsonSerializer.Deserialize<List<Student>>(json, _jsonOptions) ?? new List<Student>();
        return GetAll();
    }

    public async Task<bool> SaveToFileAsync(IEnumerable<Student> students, string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(students, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            return false;
        }
    }

    public void SaveToJson(string filePath)
    {
        var json = JsonSerializer.Serialize(_allStudents, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public List<Student> FilterByLinq(SearchCriteria criteria)
    {
        return FilterByLinq(_allStudents, criteria);
    }

    public List<Student> FilterByLinq(IEnumerable<Student> source, SearchCriteria criteria)
    {
        if (criteria == null || criteria.IsEmpty())
            return source.ToList();

        string searchVal = criteria.SearchValue.Trim();

        return source.Where(s => criteria.AttributeName switch
        {
            "FullName" => s.FullName.Contains(searchVal, StringComparison.OrdinalIgnoreCase),
            "Faculty" => s.Faculty.Contains(searchVal, StringComparison.OrdinalIgnoreCase),
            "Course" => int.TryParse(searchVal, out int course) && s.Course == course,
            "Room" => int.TryParse(searchVal, out int room) && s.Room == room,
            "Department" => s.Department.Contains(searchVal, StringComparison.OrdinalIgnoreCase),
            "CheckInDate" => s.CheckInDate.Contains(searchVal, StringComparison.OrdinalIgnoreCase),
            "CheckOutDate" => s.CheckOutDate.Contains(searchVal, StringComparison.OrdinalIgnoreCase),
            _ => true
        }).ToList();
    }

    public void AddStudent(Student student)
    {
        _allStudents.Add(student);
    }

    public void UpdateStudent(Student student)
    {
        var existing = _allStudents.FirstOrDefault(s => s.Id == student.Id);
        if (existing != null)
        {
            existing.FullName = student.FullName;
            existing.Faculty = student.Faculty;
            existing.Course = student.Course;
            existing.Room = student.Room;
            existing.Department = student.Department;
            existing.CheckInDate = student.CheckInDate;
            existing.CheckOutDate = student.CheckOutDate;
        }
    }

    public void RemoveStudent(Student student)
    {
        var target = _allStudents.FirstOrDefault(s => s.Id == student.Id);
        if (target != null)
        {
            _allStudents.Remove(target);
        }
    }
}