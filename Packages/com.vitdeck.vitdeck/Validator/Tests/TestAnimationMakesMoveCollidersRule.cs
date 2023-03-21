using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitDeck.Validator.Test
{
    public class TestAnimationMakesMoveCollidersRule
    {
        [Test]
        public void TestValidate()
        {
            var target = new ValidationTargetFinder()
                .Find("Assets/VitDeck/Validator/Tests/Data/AnimationMakesMoveCollidersRule", true);

            var result = new AnimationMakesMoveCollidersRule("")
                .Validate(target);

            var errors = result.Issues.Where(issue => issue.level == IssueLevel.Error);

            Assert.That(errors.Count(DetectedDontMoveCollider), Is.Zero);
            Assert.That(errors.Count(DetectedTransformWithoutCollider), Is.Zero);
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator")), Is.EqualTo(5));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (OverrideController)")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (No Node Connection)")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (Layered)")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (SubState)")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (Blending)")), Is.EqualTo(2));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animation")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animation (Has In Array)")), Is.EqualTo(1));
            Assert.That(errors.Count(issue => IsRootGameobjectNameOf(issue, "Animator (With Rigidbody)")), Is.EqualTo(0));

        }

        private bool DetectedDontMoveCollider(Issue issue)
        {
            return issue.target.name.Contains("FixedCollider");
        }

        private bool DetectedTransformWithoutCollider(Issue issue)
        {
            return issue.target.name.Contains("NoCollider");
        }

        private bool IsRootGameobjectNameOf(Issue issue, string expectedName)
        {
            var component = issue.target as Component;
            if (component != null)
            {
                return component.transform.root.name == expectedName;
            }

            return false;
        }
    }
}