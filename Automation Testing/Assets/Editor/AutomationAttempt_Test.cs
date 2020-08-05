using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class AutomationAttempt_Test
{
    [Test]
    public void Do_Test()
    {
        int a = 10;
        Assert.That(a, Is.EqualTo(11));
    }
}
