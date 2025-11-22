
namespace BlApi;

public interface ICourier//כרגע מכיל צפייה עדכון יצירה ומחיקה של שליחים
{
    void Create(BO.Courier boCourier);
    BO.Courier? Read(int id);
    /// <summary>
    /// Retrieves a collection of all couriers, optionally sorted and filtered based on specified criteria.
    /// </summary>
    /// <param name="sort">The field by which to sort the couriers. If <see langword="null"/>, no sorting is applied.</param>
    /// <param name="filter">The field by which to filter the couriers. If <see langword="null"/>, no filtering is applied.</param>
    /// <param name="value">The value to use for filtering. This parameter is used only if <paramref name="filter"/> is specified.</param>
    /// <returns>An enumerable collection of <see cref="BO.CourierInList"/> representing the couriers. The collection may be
    /// empty if no couriers match the criteria.</returns>
    IEnumerable<BO.CourierInList> ReadAll(//מתודה של צפייה בכל השליחים עם אפשרות למיין ולסנן על פי קריטריונים שנבחרו
        BO.CourierFieldSort? sort = null,
        BO.CourierFieldFilter? filter = null,
        object? value = null);

    void Update(BO.Courier boCourier);
    void Delete(int id);

   // void RegisterStudentToCourse(int studentId, int courseId);
   // void UnRegisterStudentFromCourse(int studentId, int courseId);

   // IEnumerable<BO.CourseInList> GetRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);
  //  IEnumerable<BO.CourseInList> GetUnRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);

   // BO.StudentGradeSheet GetGradeSheetPerStudent(int studentId, BO.Year year = BO.Year.None);
   // void UpdateGrade(int studentId, int courseId, double grade);

}
