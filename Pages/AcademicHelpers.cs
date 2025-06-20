using AcademicManagement;

namespace Lab3.Pages
{
    public static class AcademicHelpers
    {
        public static void SaveOrUpdateGrade(string studentId, string courseCode, double grade)
        {
            var records = AcademicManagement.DataAccess.GetAcademicRecordsByStudentId(studentId);
            var record = records.FirstOrDefault(r => r.CourseCode == courseCode);

            if (record != null)
            {
                record.Grade = grade;
            }
            else
            {
                var newRecord = new AcademicRecord(studentId, courseCode)
                {
                    Grade = grade
                };

                AcademicManagement.DataAccess.AddAcademicRecord(newRecord);
            }
        }
    }
}
