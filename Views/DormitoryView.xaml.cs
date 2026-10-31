using lab3.Models;
using lab3.Services;

namespace lab3.Views;

public partial class DormitoryView : ContentPage
{
    private readonly JsonDormitoryManager _manager = new();
    private List<Student> _currentDisplayedStudents = new();
    private readonly List<string> _appliedFiltersHistory = new();

    private SearchCriteria? _currentCriteria;

    public DormitoryView()
    {
        InitializeComponent();
    }

    private void DisplayStudents(List<Student> students)
    {
        _currentDisplayedStudents = students ?? new List<Student>();
        StudentsCollection.ItemsSource = null;
        StudentsCollection.ItemsSource = _currentDisplayedStudents;
        CountLabel.Text = $"Записів: {_currentDisplayedStudents.Count}";

        ActiveFiltersLabel.Text = _appliedFiltersHistory.Count > 0
            ? "Активні фільтри: " + string.Join(" ➔ ", _appliedFiltersHistory)
            : "Активні фільтри: Немає (всі записи)";
    }

    private void OnSearchClicked(object? sender, EventArgs e)
    {
        string? selectedUa = AttributePicker.SelectedItem?.ToString();
        string value = SearchValueEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(selectedUa))
        {
            DisplayAlertAsync("Увага", "Оберіть критерій для пошуку!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            DisplayAlertAsync("Увага", "Введіть значення для фільтрації!", "OK");
            return;
        }

        string internalAttribute = selectedUa switch
        {
            "ПІБ" => "FullName",
            "Факультет" => "Faculty",
            "Курс" => "Course",
            "Номер кімнати" => "Room",
            "Кафедра" => "Department",
            "Дата поселення" => "CheckInDate",
            _ => selectedUa
        };

        _currentCriteria = new SearchCriteria(internalAttribute, value);

        bool isCascade = CascadeFilterCheckBox.IsChecked && _currentDisplayedStudents.Count > 0;
        var source = isCascade ? _currentDisplayedStudents : _manager.GetAll();

        if (!isCascade)
        {
            _appliedFiltersHistory.Clear();
        }

        var result = _manager.FilterByLinq(source, _currentCriteria);
        _appliedFiltersHistory.Add($"[{selectedUa}: \"{value}\"]");

        DisplayStudents(result);
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        SearchValueEntry.Text = string.Empty;
        AttributePicker.SelectedIndex = -1;
        _appliedFiltersHistory.Clear();
        _currentCriteria = null;
        DisplayStudents(_manager.GetAll());
    }

    private async void OnLoadFileClicked(object? sender, EventArgs e)
    {
        var loaded = await _manager.OpenJsonFileAsync();
        if (loaded != null)
        {
            _appliedFiltersHistory.Clear();
            _currentCriteria = null;
            DisplayStudents(loaded);
            await DisplayAlertAsync("Успіх", $"Завантажено {loaded.Count} студентів із файлу!", "OK");
        }
    }

    private async void OnSaveFileClicked(object? sender, EventArgs e)
    {
        var dataToSave = _currentDisplayedStudents.Count > 0
            ? _currentDisplayedStudents
            : _manager.GetAll();

        if (dataToSave.Count == 0)
        {
            await DisplayAlertAsync("Увага", "Немає записів для збереження!", "OK");
            return;
        }

        string fileName = await DisplayPromptAsync(
            "Збереження JSON",
            "Введіть ім'я файлу:",
            initialValue: "dormitory_filtered.json");

        if (string.IsNullOrWhiteSpace(fileName))
            return;

        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fileName += ".json";

        string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string fullPath = Path.Combine(documentsFolder, fileName);

        bool success = await _manager.SaveToFileAsync(dataToSave, fullPath);
        if (success)
        {
            await DisplayAlertAsync("Успіх", $"Збережено {dataToSave.Count} записів у файл:\n{fullPath}", "OK");
        }
        else
        {
            await DisplayAlertAsync("Помилка", "Не вдалося зберегти файл.", "OK");
        }
    }

    private async void OnAddStudentClicked(object? sender, EventArgs e)
    {
        var editPage = new EditStudentPage(student: null, isNew: true);
        editPage.Disappearing += (s, args) =>
        {
            if (editPage.StudentData != null && !string.IsNullOrWhiteSpace(editPage.StudentData.FullName))
            {
                _manager.AddStudent(editPage.StudentData);
                _appliedFiltersHistory.Clear();
                _currentCriteria = null;
                DisplayStudents(_manager.GetAll());
            }
        };
        await Navigation.PushModalAsync(editPage);
    }

    private async void OnEditStudentClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Student student)
        {
            var editPage = new EditStudentPage(student: student, isNew: false);
            editPage.Disappearing += (s, args) =>
            {
                if (editPage.IsDeleteRequested)
                {
                    _manager.RemoveStudent(student);
                    _appliedFiltersHistory.Clear();
                    _currentCriteria = null;
                    DisplayStudents(_manager.GetAll());
                }
                else if (editPage.StudentData != null)
                {
                    _manager.UpdateStudent(editPage.StudentData);
                    _appliedFiltersHistory.Clear();
                    _currentCriteria = null;
                    DisplayStudents(_manager.GetAll());
                }
            };
            await Navigation.PushModalAsync(editPage);
        }
    }

    private async void OnDeleteStudentClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Student student)
        {
            bool confirm = await DisplayAlertAsync("Видалення", $"Видалити запис про:\n{student.FullName}?", "Так", "Ні");
            if (confirm)
            {
                _manager.RemoveStudent(student);
                _appliedFiltersHistory.Clear();
                _currentCriteria = null;
                DisplayStudents(_manager.GetAll());
            }
        }
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Про програму",
            "Лабораторна робота №3: Диспетчер для JSON\n" +
            "Студентка: Кучинська Софія\n" +
            "Курс: 2 | Група: К28\n" +
            "Рік: 2026\n\n" +
            "Опис: Програма для фільтрації через LINQ, перегляду, додавання, редагування та експорту відомостей про студентів гуртожитку КНУ.",
            "Зрозуміло"
        );
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Вихід", "Ви дійсно бажаєте вийти з програми?", "Так", "Ні");
        if (confirm)
        {
            Application.Current?.Quit();
        }
    }
}