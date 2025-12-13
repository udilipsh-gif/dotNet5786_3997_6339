

namespace BlImplementation;
using BlApi;

using Helpers;



internal class AdminImplementation : IAdmin
{
    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    public void InitializeDB()
    {
        ResetDB();
        AdminManager.InitializeDB();
    }
    public DateTime GetClock()
    {
        return AdminManager.Now;
    }
   
    public void ForwardClock(BO.TimeUnit unit)
    {

        switch (unit)
        {
            case BO.TimeUnit.MINUTE:
                AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
                break;

            case BO.TimeUnit.HOUR:
                AdminManager.UpdateClock(AdminManager.Now.AddHours(1));
                break;

            case BO.TimeUnit.DAY:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(1));

                break;

            case BO.TimeUnit.MONTH:
                AdminManager.UpdateClock(AdminManager.Now.AddMonths(1));
                break;
            case BO.TimeUnit.WEEK:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(7));
                break;

            case BO.TimeUnit.YEAR:
                AdminManager.UpdateClock(AdminManager.Now.AddYears(1));
                break;

            default:
                throw new BO.BlInvalidValueException(nameof(unit));
        }


    }
    public BO.Config GetConfig()
    {
        return AdminManager.GetConfig();
    }
    public void SetConfig(BO.Config config)
    {
        AdminManager.SetConfig(config);
    }


}
