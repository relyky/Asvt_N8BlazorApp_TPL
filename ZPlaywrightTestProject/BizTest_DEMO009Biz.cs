using Microsoft.Extensions.DependencyInjection;
using Vista.Biz.DEMO;
using Vista.Models;

namespace ZPlaywrightTestProject;

[TestClass]
public class BizTest_DEMO009Biz : BizTestBase
{
  [TestMethod]
  public void Test_QryDataList()
  {
    var bizSvc = scope.ServiceProvider.GetRequiredService<DEMO009Biz>();
    var dataList = bizSvc.QryDataList(new DEMO009QryArgs());
    Assert.IsTrue(dataList.Count > 0);
  }

  [TestMethod]
  public void Test_QryDataList2()
  {
    Assert.AreEqual("foo", "foo");
  }
}

class ATestUser : IAuthUser
{
  public AuthUser? GetCurrentUser()
  {
    return new AuthUser
    {
      UserId = "itest",
      UserName = "艾測試",
      AuthMenu = new()
    };
  }

  public Task<AuthUser?> GetCurrentUserAsync()
  {
    return Task.FromResult(this.GetCurrentUser());
  }
}
