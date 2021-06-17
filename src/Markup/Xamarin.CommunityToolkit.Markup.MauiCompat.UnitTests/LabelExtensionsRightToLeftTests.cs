﻿using NUnit.Framework;
using Xamarin.CommunityToolkit.Markup.MauiCompat.RightToLeft;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Xamarin.CommunityToolkit.Markup.MauiCompat.UnitTests
{
    [TestFixture]
    public class LabelExtensionsRightToLeftTests : MarkupBaseTestFixture<Label>
    {
        [Test]
        public void TextLeft()
            => TestPropertiesSet(l => l?.TextLeft(), (Label.HorizontalTextAlignmentProperty, TextAlignment.Start, TextAlignment.End));

        [Test]
        public void TextRight()
            => TestPropertiesSet(l => l?.TextRight(), (Label.HorizontalTextAlignmentProperty, TextAlignment.End, TextAlignment.Start));

        [Test]
        public void SupportDerivedFromLabel() => Assert.IsInstanceOf<DerivedFromLabel>(new DerivedFromLabel().TextLeft().TextRight());

        class DerivedFromLabel : Label { }
    }
}