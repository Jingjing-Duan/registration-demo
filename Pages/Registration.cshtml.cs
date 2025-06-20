using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AcademicManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.Connections;

namespace Lab3.Pages
{
    public class RegistrationModel : PageModel
    {

        [BindProperty]
        public string SelectedStudentId { get; set; } = "-1";//用户在页面上选中的学生 ID

        [BindProperty]
        public List<SelectListItem> CourseSelections { get; set; } //用户在页面上选中的学生 ID

        public List<Course> AvailableCourses { get; set; } = new();

        [BindProperty]
        public List<string> SelectedCourseCodes { get; set; }

        public List<Course> RegisteredCourses { get; set; } = new(); //成功注册后，用于显示学生已选课程
        [BindProperty]
        public List<AcademicRecord> AcademicRecords { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "asc";

        public bool ShowCourses { get; set; } = false;

        public string ErrorMessage { get; set; }
        public string ShowMessage { get; set; }

        public List<Student> allStudents = DataAccess.GetAllStudents();
        public List<Course> allCourses = DataAccess.GetAllCourses();

        public void OnGet()
        {
            SelectedStudentId = HttpContext.Session.GetString("SelectedStudentId") ?? "-1";
            SortField = HttpContext.Session.GetString("SortField") ?? "";
            SortOrder = HttpContext.Session.GetString("SortOrder") ?? "asc";

            if (SelectedStudentId != "-1")
            {
                // 自动恢复数据展示（可选）
                AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);
            }
        }

        public IActionResult OnPostStudentSelected()
        {
            HttpContext.Session.SetString("SelectedStudentId", SelectedStudentId);
            HttpContext.Session.SetString("SortField", SortField ?? "");
            HttpContext.Session.SetString("SortOrder", SortOrder ?? "asc");

            if (SelectedStudentId == "-1")
            {
                Console.WriteLine("User selected 'Choose a student'");
                // 重置状态
                ShowCourses = false;
                RegisteredCourses = new List<Course>();
                AvailableCourses = new List<Course>();
                AcademicRecords = new List<AcademicRecord>();
                ErrorMessage = "";
                return Page();
            }

            // 1. 校验学生是否被选中
            if (string.IsNullOrEmpty(SelectedStudentId) || SelectedStudentId == "-1")
            {
                ErrorMessage = "You must select a student.";
                return Page();
            }

            // 2. 获取该学生已注册课程的 CourseCode 列表
            var registeredRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);
            var registeredCodes = registeredRecords.Select(r => r.CourseCode).ToList();
            var allCourses = DataAccess.GetAllCourses();

            AcademicRecords = registeredRecords;

            if (registeredCodes.Any())
            {
                // 4. 构造 CourseSelections，供 checkbox 使用
                RegisteredCourses = registeredRecords
                    .Select(r => allCourses.FirstOrDefault(c => c.CourseCode == r.CourseCode))
                    .Where(c => c != null).ToList();

                ShowCourses = false;
            }
            else
            {
                AvailableCourses = allCourses;

                CourseSelections = AvailableCourses.Select(c => new SelectListItem
                {
                    Text = $"{c.CourseTitle} ({c.CourseCode})",
                    Value = c.CourseCode,
                    Selected = false
                }).ToList();

                ShowCourses = true; // ✅ 显示 checkbox 区域
            }

            return Page();
        }

        public IActionResult OnPostRegister()
        {
            // 1. 再次获取学生列表用于页面下拉框
            if (string.IsNullOrEmpty(SelectedStudentId) || SelectedStudentId == "-1")
            {
                ErrorMessage = "Please select a student.";
                return Page();
            }

            if (CourseSelections == null || !CourseSelections.Any(s => s.Selected))
            {
                ErrorMessage = "Please select at least one course.";
                // 重新加载课程列表（已注册的排除）
                var registeredCodes = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId)
                                                .Select(r => r.CourseCode).ToList();
                AvailableCourses = DataAccess.GetAllCourses()
                    .Where(c => !registeredCodes.Contains(c.CourseCode)).ToList();

                CourseSelections = AvailableCourses.Select(c => new SelectListItem
                {
                    Text = $"{c.CourseTitle} ({c.CourseCode})",
                    Value = c.CourseCode,
                    Selected = false
                }).ToList();

                ShowCourses = true;
                return Page();
            }

            // 2. 遍历选中的项并注册
            foreach (var item in CourseSelections)
            {
                if (item.Selected)
                {
                    var record = new AcademicRecord(SelectedStudentId, item.Value);
                    DataAccess.AddAcademicRecord(record);
                }
            }

            // 3. 获取所有已注册课程用于显示

            AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);

            return Page();
        }

        public IActionResult OnPostSubmitGrades()
        {
            if (string.IsNullOrEmpty(SelectedStudentId) || AcademicRecords == null || AcademicRecords.Count == 0)
            {
                ErrorMessage = "Please select a student and enter grades.";
                return Page();
            }

            foreach (var record in AcademicRecords)
            {
                // 如果用户留空不填，可以跳过或设置默认值
                if (record.Grade >= 0 && record.Grade <= 100)
                {
                    AcademicHelpers.SaveOrUpdateGrade(SelectedStudentId, record.CourseCode, record.Grade);
                }
            }

            // 成功保存后，重新加载 AcademicRecords
            AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);
            //AvailableCourses = DataAccess.GetAvailableCourses(SelectedStudentId);
            ShowCourses = !AcademicRecords.Any();

            return Page();
        }
        public IActionResult OnGetStudentSelected(string selectedStudentId, string sortField, string sortOrder)
        {
            SelectedStudentId = selectedStudentId;
            SortField = sortField;
            SortOrder = sortOrder;

            HttpContext.Session.SetString("SelectedStudentId", SelectedStudentId);
            HttpContext.Session.SetString("SortField", SortField ?? "");
            HttpContext.Session.SetString("SortOrder", SortOrder ?? "asc");

            if (string.IsNullOrEmpty(SelectedStudentId) || SelectedStudentId == "-1")
            {
                ErrorMessage = "You must select a student.";
                return Page();
            }

            // 获取已注册课程
            AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);

            // 获取未注册课程
            var registeredCodes = AcademicRecords.Select(r => r.CourseCode).ToList();
            AvailableCourses = DataAccess.GetAllCourses()
                                         .Where(c => !registeredCodes.Contains(c.CourseCode))
                                         .ToList();

            // 构建 CourseSelections 列表
            CourseSelections = AvailableCourses.Select(c => new SelectListItem
            {
                Text = $"{c.CourseTitle} ({c.CourseCode})",
                Value = c.CourseCode,
                Selected = false
            }).ToList();

            // 排序 CourseSelections（用于注册）
            if (ShowCourses || !AcademicRecords.Any())
            {
                if (SortField == "CourseCode")
                    if (SortOrder == "asc")
                        AvailableCourses.Sort((c1, c2) => c1.CourseCode.CompareTo(c2.CourseCode));
                    else
                        AvailableCourses.Sort((c1, c2) => c2.CourseCode.CompareTo(c1.CourseCode));

                else if (SortField == "CourseTitle")
                    if (SortOrder == "asc")
                        AvailableCourses.Sort((c1, c2) => c1.CourseTitle.CompareTo(c2.CourseTitle));
                    else
                        AvailableCourses.Sort((c1, c2) => c2.CourseTitle.CompareTo(c1.CourseTitle));

                // 重新构建已排序的 SelectList
                CourseSelections = AvailableCourses.Select(c => new SelectListItem
                {
                    Text = $"{c.CourseTitle} ({c.CourseCode})",
                    Value = c.CourseCode,
                    Selected = false
                }).ToList();

                ShowCourses = true;
            }

            // 排序 AcademicRecords（用于成绩）
            if (AcademicRecords != null && AcademicRecords.Any())
            {
                if (SortField == "CourseCode")
                    if (SortOrder == "asc")
                        AcademicRecords.Sort((c1, c2) => c1.CourseCode.CompareTo(c2.CourseCode));
                    else
                        AcademicRecords.Sort((c1, c2) => c2.CourseCode.CompareTo(c1.CourseCode));

                else if (SortField == "Grade")
                    if (SortOrder == "asc")
                        AcademicRecords.Sort((c1, c2) => c1.Grade.CompareTo(c2.Grade));
                    else
                        AcademicRecords.Sort((c1, c2) => c2.Grade.CompareTo(c1.Grade));
                else if (SortField == "CourseTitle")
                {
                    var allCourses = DataAccess.GetAllCourses();

                    if (SortOrder == "asc")
                    {
                        AcademicRecords.Sort((r1, r2) =>
                        {
                            var title1 = allCourses.FirstOrDefault(c => c.CourseCode == r1.CourseCode)?.CourseTitle;
                            var title2 = allCourses.FirstOrDefault(c => c.CourseCode == r2.CourseCode)?.CourseTitle;
                            return string.Compare(title1, title2); // ascending
                        });
                    }
                    else
                    {
                        AcademicRecords.Sort((r1, r2) =>
                        {
                            var title1 = allCourses.FirstOrDefault(c => c.CourseCode == r1.CourseCode)?.CourseTitle;
                            var title2 = allCourses.FirstOrDefault(c => c.CourseCode == r2.CourseCode)?.CourseTitle;
                            return string.Compare(title2, title1); // descending
                        });
                    }
                }
            }

            return Page();
        }
    }

    }
