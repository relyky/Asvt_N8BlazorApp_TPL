using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.Models;

namespace ZPlaywrightTestProject;

[TestClass]
public class UnitTest_Vista_DB_Utils
{
  [TestMethod]
  public void Test_Clsx()
  {
    Assert.AreEqual("foo",
      Utils.Clsx("foo", true));

    Assert.AreEqual("",
      Utils.Clsx("foo", false));

    Assert.AreEqual("base foo",
      Utils.Clsx("base", "foo", true));

    Assert.AreEqual("base",
      Utils.Clsx("base", "foo", false));

    Assert.AreEqual("base base2 foo bar",
      Utils.Clsx("base base2", ("foo", true), ("bar", true)));

    Assert.AreEqual("base base2 bar",
      Utils.Clsx("base base2", ("foo", false), ("bar", true)));

    Assert.AreEqual("base base2",
      Utils.Clsx("base base2", ("foo", false), ("bar", false)));

    Assert.AreEqual("",
      Utils.Clsx(null!, ("foo", false)));

    Assert.AreEqual("foo show-me",
      Utils.Clsx(null!, ("foo", true), "show-me"));

    //Class=@Utils.Clsx(("mud-input-required", Required), Class!)
    Assert.AreEqual("mud-input-required show-me",
      Utils.Clsx(("mud-input-required", true), "show-me"));

    Assert.AreEqual("mud-input-required",
      Utils.Clsx(("mud-input-required", true), null!));

    Assert.AreEqual("show-me",
      Utils.Clsx(("mud-input-required", false), "show-me"));

    Assert.AreEqual("",
      Utils.Clsx(("mud-input-required", false), null!));
  }

  [TestMethod]
  public void Test_UrlCombine()
  {
    Assert.AreEqual("http://foo.com/",
      Utils.UrlCombine("http://foo.com/"));

    Assert.AreEqual("http://foo.com/foo/bar/baz",
      Utils.UrlCombine("http://foo.com", "foo", "bar", "baz"));

    Assert.AreEqual("http://foo.com/foo/bar/baz/",
      Utils.UrlCombine("http://foo.com/", "/foo/", "/bar/", "/baz/"));

    Assert.AreEqual("http://foo.com/foo/baz",
      Utils.UrlCombine("http://foo.com", "foo", null!, "baz"));

    Assert.AreEqual("http://foo.com/foo/baz",
      Utils.UrlCombine("http://foo.com", "foo", string.Empty, "baz"));

    Assert.AreEqual("http://foo.com/foo/baz",
      Utils.UrlCombine("http://foo.com", "foo", "", "baz"));

    Assert.AreEqual("http://foo.com/foo",
      Utils.UrlCombine("http://foo.com", "foo", string.Empty));
  }

}
