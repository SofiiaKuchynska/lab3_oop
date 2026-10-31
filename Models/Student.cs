using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace lab3.Models;

public class Student : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public Guid Id { get; set; } = Guid.NewGuid();

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
    }

    private string _faculty = string.Empty;
    public string Faculty
    {
        get => _faculty;
        set { _faculty = value; OnPropertyChanged(); }
    }

    private int _course;
    public int Course
    {
        get => _course;
        set { _course = value; OnPropertyChanged(); }
    }

    private int _room;
    public int Room
    {
        get => _room;
        set { _room = value; OnPropertyChanged(); }
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set { _department = value; OnPropertyChanged(); }
    }

    private string _checkInDate = string.Empty;
    public string CheckInDate
    {
        get => _checkInDate;
        set { _checkInDate = value; OnPropertyChanged(); }
    }

    private string _checkOutDate = string.Empty;
    public string CheckOutDate
    {
        get => _checkOutDate;
        set { _checkOutDate = value; OnPropertyChanged(); }
    }

    public Student Clone() => (Student)MemberwiseClone();
}