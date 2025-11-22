
namespace BlApi;

public interface IOrder
{
    void Create(BO.Order boCourier);
    BO.Order? Read(int id);
    
    //IEnumerable<BO.CourierInList> ReadAll(//מתודה של צפייה בכל השליחים עם אפשרות למיין ולסנן על פי קריטריונים שנבחרו
    //    BO.CourierFieldSort? sort = null,
    //    BO.CourierFieldFilter? filter = null,
    //    object? value = null);

    void Update(BO.Order boCourier);
    void Delete(int id);

    // void RegisterStudentToCourse(int studentId, int courseId);
    // void UnRegisterStudentFromCourse(int studentId, int courseId);

    // IEnumerable<BO.CourseInList> GetRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);
    //  IEnumerable<BO.CourseInList> GetUnRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);

    // BO.StudentGradeSheet GetGradeSheetPerStudent(int studentId, BO.Year year = BO.Year.None);
    // void UpdateGrade(int studentId, int courseId, double grade);

}

}
