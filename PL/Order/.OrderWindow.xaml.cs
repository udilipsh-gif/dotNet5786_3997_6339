using PL.Courier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Order;

/// <summary>
/// Interaction logic for _.xaml
/// </summary>
public partial class OrderWindow : Window
{




    /// <summary>
    /// Business logic layer instance for accessing courier operations.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


    private int CURRENT_ID = 0;

    /// <summary>
    /// The ID of the currently logged-in manager.
    /// </summary>
    private readonly int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId!;


    //public IEnumerable<BO.TypeOfOrder> TypeOfOrderValues
    //{
    //    get
    //    {
    //        return Enum.GetValues(typeof(BO.TypeOfOrder)).Cast<BO.TypeOfOrder>();
    //    }
    //}
    public IEnumerable<BO.ScheduleStatus> ScheduleStatusValues
    {
        get
        {
            return Enum.GetValues(typeof(BO.ScheduleStatus)).Cast<BO.ScheduleStatus>();
        }
    }
    public IEnumerable<BO.OrderStatus> OrderStatusValues
    {
        get
        {
            return Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>();
        }
    }
    public OrderWindow(int id=0)
    {
        InitializeComponent();
        CURRENT_ID = id;
    }

    public BO.Order CurrentOrder
    {
        get => (BO.Order)GetValue(CurrentOrderProperty);
        set => SetValue(CurrentOrderProperty, value);
    }
    public static readonly DependencyProperty CurrentOrderProperty =
         DependencyProperty.Register("CurrentOrder", typeof(BO.Order),
             typeof(OrderWindow), new PropertyMetadata(new BO.Order()
             {
                 Id = 0,
                 Name = "",
                 Phone = "",
                 Addres = "",
                 Details = "",
                 OrderStatus = BO.OrderStatus.OPEN,
                 OrderDate = DateTime.Now,
                 TimeLeftForDelivery = s_bl.Admin.GetConfig().MaxDeliveryTime,
                 Weight = 0,
                 Distance = 0,
                 TypeOfOrder = BO.TypeOfOrder.STANDART,
                 ScheduleStatus = BO.ScheduleStatus.ONTYME,
                 EstimatedDeliveryTime = null,
                 MaxDeliveryTime = DateTime.Now + s_bl.Admin.GetConfig().MaxDeliveryTime



             }));





}

