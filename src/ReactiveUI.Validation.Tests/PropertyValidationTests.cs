// Copyright (c) 2021 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using ReactiveUI.Validation.Comparators;
using ReactiveUI.Validation.Components;
using ReactiveUI.Validation.States;
using ReactiveUI.Validation.Tests.Models;
using Xunit;

namespace ReactiveUI.Validation.Tests;

/// <summary>
/// Tests for <see cref="BasePropertyValidation{TViewModel}"/>.
/// </summary>
public class PropertyValidationTests
{
    /// <summary>
    /// Verifies if the default state is true.
    /// </summary>
    [Fact]
    public void ValidModelDefaultStateTest()
    {
        var model = CreateDefaultValidModel();

        using var validation = new BasePropertyValidation<TestViewModel, string>(
            model,
            vm => vm.Name,
            n => !string.IsNullOrEmpty(n),
            "broken");

        Assert.True(validation.IsValid);
        Assert.True(string.IsNullOrEmpty(validation.Text?.ToSingleLine()));
    }

    /// <summary>
    /// Verifies if the state transition is valid when the IsValid property changes.
    /// </summary>
    [Fact]
    public void StateTransitionsWhenValidityChangesTest()
    {
        const string testValue = "test";

        var model = new TestViewModel();

        using var validation = new BasePropertyValidation<TestViewModel, string>(
            model,
            vm => vm.Name,
            n => n is not null && n.Length >= testValue.Length,
            "broken");

        bool? lastVal = null;

        var unused = validation
            .ValidationStatusChange
            .Subscribe(v => lastVal = v.IsValid);

        Assert.False(validation.IsValid);
        Assert.False(lastVal);
        Assert.True(lastVal.HasValue);

        model.Name = testValue + "-" + testValue;

        Assert.True(validation.IsValid);
        Assert.True(lastVal);
    }

    /// <summary>
    /// Verifies if the validation message is the expected.
    /// </summary>
    [Fact]
    public void PropertyContentsProvidedToMessageTest()
    {
        const string testValue = "bongo";

        var model = new TestViewModel();

        using var validation = new BasePropertyValidation<TestViewModel, string>(
            model,
            vm => vm.Name,
            n => n is not null && n.Length > testValue.Length,
            v => $"The value '{v}' is incorrect");

        model.Name = testValue;

        Assert.Equal("The value 'bongo' is incorrect", validation.Text?.ToSingleLine());
    }

    /// <summary>
    /// Verifies that validation message updates are correctly propagated.
    /// </summary>
    [Fact]
    public void MessageUpdatedWhenPropertyChanged()
    {
        const string testRoot = "bon";
        const string testValue = testRoot + "go";

        var model = new TestViewModel();

        using var validation = new BasePropertyValidation<TestViewModel, string>(
            model,
            vm => vm.Name,
            n => n is not null && n.Length > testValue.Length,
            v => $"The value '{v}' is incorrect");

        model.Name = testValue;

        var changes = new List<IValidationState>();

        validation.ValidationStatusChange.Subscribe(v => changes.Add(v));

        Assert.Equal("The value 'bongo' is incorrect", validation.Text?.ToSingleLine());
        Assert.Single(changes);
        Assert.Equal(new ValidationState(false, "The value 'bongo' is incorrect"), changes[0], new ValidationStateComparer());

        model.Name = testRoot;

        Assert.Equal("The value 'bon' is incorrect", validation.Text?.ToSingleLine());
        Assert.Equal(2, changes.Count);
        Assert.Equal(new ValidationState(false, "The value 'bon' is incorrect"), changes[1], new ValidationStateComparer());
    }

    /// <summary>
    /// Verifies that validation message changes if one validation is valid but the other one is not.
    /// </summary>
    [Fact]
    public void DualStateMessageTest()
    {
        const string testRoot = "bon";
        const string testValue = testRoot + "go";

        var model = new TestViewModel { Name = testValue };

        using var validation = new BasePropertyValidation<TestViewModel, string>(
            model,
            vm => vm.Name,
            n => n is not null && n.Length > testRoot.Length,
            (p, v) => v ? "cool" : $"The value '{p}' is incorrect");

        Assert.Equal("cool", validation.Text?.ToSingleLine());

        model.Name = testRoot;

        Assert.Equal("The value 'bon' is incorrect", validation.Text?.ToSingleLine());
    }

    private static TestViewModel CreateDefaultValidModel() => new() { Name = "name" };
}