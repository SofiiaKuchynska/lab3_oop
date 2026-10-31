using lab3.Models;

namespace lab3.Views;

public partial class EditStudentPage : ContentPage
{
    public Student StudentData { get; private set; }
    public bool IsDeleteRequested { get; private set; }
    private readonly bool _isNew;

    public EditStudentPage(Student? student = null, bool isNew = false)
    {
        InitializeComponent();
        _isNew = isNew;

        if (student != null)
        {
            StudentData = student.Clone();
            FullNameEntry.Text = student.FullName;
            FacultyEntry.Text = student.Faculty;
            CourseEntry.Text = student.Course > 0 ? student.Course.ToString() : string.Empty;
            RoomEntry.Text = student.Room > 0 ? student.Room.ToString() : string.Empty;
            DeptEntry.Text = student.Department;
            CheckInEntry.Text = student.CheckInDate;
            CheckOutEntry.Text = student.CheckOutDate;
        }
        else
        {
            StudentData = new Student();
        }

        PageHeaderLabel.Text = _isNew ? "Новий запис студента" : "Редагування студента";
        DeleteBtn.IsVisible = !_isNew;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "ПІБ студента є обов'язковим!", "OK");
            return;
        }

        int.TryParse(CourseEntry.Text, out int course);
        int.TryParse(RoomEntry.Text, out int room);

        StudentData.FullName = FullNameEntry.Text.Trim();
        StudentData.Faculty = FacultyEntry.Text?.Trim() ?? string.Empty;
        StudentData.Course = course;
        StudentData.Room = room;
        StudentData.Department = DeptEntry.Text?.Trim() ?? string.Empty;
        StudentData.CheckInDate = CheckInEntry.Text?.Trim() ?? string.Empty;
        StudentData.CheckOutDate = CheckOutEntry.Text?.Trim() ?? string.Empty;

        await Navigation.PopModalAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Підтвердження", $"Ви дійсно хочете видалити студента {StudentData.FullName}?", "Так, видалити", "Ні");
        if (!confirm) return;

        IsDeleteRequested = true;
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        StudentData = null!;
        await Navigation.PopModalAsync();
    }
}