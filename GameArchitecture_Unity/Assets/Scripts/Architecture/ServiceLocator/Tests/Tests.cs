using System.Collections;
using System.Collections.Generic;
using System.Text;
using Architecture.Services;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Tests
{
    const string testStr = "Hello";

    [Test]
    public void Test01()
    {
        ServiceLocator.Clean();
        StringBuilder builder = ServiceLocator.Get<StringBuilder>();
        Assert.IsNull(builder);
        ServiceLocator.Clean();
    }

    [Test]
    public void Test02()
    {
        ServiceLocator.Clean();
        StringBuilder builder = new StringBuilder(testStr);
        ServiceLocator.Set(builder);
        StringBuilder service = ServiceLocator.Get<StringBuilder>();
        Assert.IsTrue(service == builder);
        ServiceLocator.Clean();
    }

    [Test]
    public void Test03()
    {
        ServiceLocator.Clean();
        StringBuilder builder = new StringBuilder(testStr);
        ServiceLocator.Set(builder);
        StringBuilder service = ServiceLocator.Get<StringBuilder>();
        Assert.IsTrue(service.ToString() == builder.ToString());
        ServiceLocator.Clean();
    }
}
