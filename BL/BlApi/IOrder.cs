using BO;

namespace BlApi;

public interface IOrder
{
    void Create(int id, BO.Order boCourier);
    BO.Order? Read(int id, int OrderId);
    
    //IEnumerable<BO.CourierInList> ReadAll(//מתודה של צפייה בכל השליחים עם אפשרות למיין ולסנן על פי קריטריונים שנבחרו
    //    BO.CourierFieldSort? sort = null,
    //    BO.CourierFieldFilter? filter = null,
    //    object? value = null);
    
    int[] GetAllOrderStatistic(int id);

    BO.OrderInList ReadAll(int id, BO.OrderInListField? filter, object? value, BO.OrderInListField? sort);

    void Update(int id, BO.Order boOrder);

    void Cancel(int id, int orderId);

    void Deliver(int id, int courierId, int orderId); //סגירת הזמנה כמסופקת

    void StartDelivery(int id, int courierId, int orderId);

    void Delete(int id, int orderId);

    BO.ClosedDeliveryInList GetClosed(int id, int courierId, ClosedDeliveryInListField? filter, ClosedDeliveryInListField sort);

    BO.OpenOrderInList GetOpen(int id, int courierId, OpenOrderInListField? filter, OpenOrderInListField sort);



    // void RegisterStudentToCourse(int studentId, int courseId);
    // void UnRegisterStudentFromCourse(int studentId, int courseId);

    // IEnumerable<BO.CourseInList> GetRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);
    //  IEnumerable<BO.CourseInList> GetUnRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);

    // BO.StudentGradeSheet GetGradeSheetPerStudent(int studentId, BO.Year year = BO.Year.None);
    // void UpdateGrade(int studentId, int courseId, double grade);

}


