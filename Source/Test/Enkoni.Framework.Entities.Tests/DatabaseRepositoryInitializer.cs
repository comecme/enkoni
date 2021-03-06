﻿using System.Collections.Generic;
using System.Data.Entity;

namespace Enkoni.Framework.Entities.Tests {
  /// <summary>The testcategories that are supported by the testinitializer.</summary>
  public enum TestCategory {
    /// <summary>Testcases that test the retrieve-functions of the repository.</summary>
    Retrieve,

    /// <summary>Testcases that test the retrieve-functions of the repository with an empty datasource.</summary>
    RetrieveEmpty,

    /// <summary>Testcases that test the sorting-functions of the repository.</summary>
    Sorting,

    /// <summary>Testcases that test the storage-functions of the repository.</summary>
    Storage
  }

  /// <summary>A helper class to support the test cases.</summary>
  public class DatabaseRepositoryInitializer : DropCreateDatabaseAlways<DatabaseRepositoryTestContext> {
    /// <summary>Defines the testcategory for which the database must be initialized.</summary>
    private TestCategory category;

    /// <summary>Initializes a new instance of the <see cref="DatabaseRepositoryInitializer"/> class.</summary>
    /// <param name="category">The test category.</param>
    public DatabaseRepositoryInitializer(TestCategory category) {
      this.category = category;
    }

    /// <summary>Seeds the database with the initial data.</summary>
    /// <param name="context">The context that gives access to the datasource.</param>
    protected override void Seed(DatabaseRepositoryTestContext context) {
      switch(this.category) {
        case TestCategory.Retrieve:
          SeedRetrieveCategory(context);
          break;
        case TestCategory.RetrieveEmpty:
          SeedRetrieveEmptyCategory(context);
          break;
        case TestCategory.Sorting:
          SeedSortingCategory(context);
          break;
        case TestCategory.Storage:
          SeedStorageCategory(context);
          break;
      }
    }

    /// <summary>Seeds the database for the 'retrieve' testcases.</summary>
    /// <param name="context">The context that gives access to the datasource.</param>
    private static void SeedRetrieveCategory(DatabaseRepositoryTestContext context) {
      List<TestDummy> testInstances = new List<TestDummy>();
      testInstances.Add(new TestDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "\"Row3\"", NumericValue = 7, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "\"Row4\"", NumericValue = 2, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "\"Row5\"", NumericValue = 5, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "\"Row6\"", NumericValue = 1, BooleanValue = true });
      testInstances.ForEach(instance => context.TestDummies.Add(instance));

      List<TestSubDummy> subInstances = new List<TestSubDummy>();
      subInstances.Add(new TestSubDummy { TextValue = "Row1.A" });
      subInstances.Add(new TestSubDummy { TextValue = "Row1.B" });
      subInstances.Add(new TestSubDummy { TextValue = "Row1.C" });
      subInstances.Add(new TestSubDummy { TextValue = "Row2.A" });
      subInstances.Add(new TestSubDummy { TextValue = "Row3.A" });
      subInstances.Add(new TestSubDummy { TextValue = "Row3.B" });

      List<TestParentDummy> parentInstances = new List<TestParentDummy>();
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false, Children = new List<TestSubDummy> { subInstances[0], subInstances[1], subInstances[2] } });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true, Children = new List<TestSubDummy> { subInstances[3] } });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row3\"", NumericValue = 7, BooleanValue = true, Children = new List<TestSubDummy> { subInstances[4], subInstances[5] } });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row4\"", NumericValue = 2, BooleanValue = true });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row5\"", NumericValue = 5, BooleanValue = false });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row6\"", NumericValue = 1, BooleanValue = true });
      parentInstances.ForEach(instance => context.TestParentDummies.Add(instance));
            
      List<CustomMappedTestDummy> customMappedTestInstances = new List<CustomMappedTestDummy>();
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row3\"", NumericValue = 7, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row4\"", NumericValue = 2, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row5\"", NumericValue = 5, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row6\"", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.ForEach(instance => context.CustomMappedTestDummies.Add(instance));
    }

    /// <summary>Seeds the database for the 'retrieve' testcases.</summary>
    /// <param name="context">The context that gives access to the datasource.</param>
    private static void SeedSortingCategory(DatabaseRepositoryTestContext context) {
      List<TestDummy> testInstances = new List<TestDummy>();
      testInstances.Add(new TestDummy { TextValue = "aabcdef", NumericValue = 3, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "abcdefg", NumericValue = 3, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "aadefgh", NumericValue = 7, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "abefghi", NumericValue = 2, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "bbcdefg", NumericValue = 5, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "bbdefgh", NumericValue = 1, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "bbefghi", NumericValue = 1, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "bbfghij", NumericValue = 1, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "acdefgh", NumericValue = 5, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "ccefghi", NumericValue = 1, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "acfghij", NumericValue = 1, BooleanValue = true });
      testInstances.Add(new TestDummy { TextValue = "ccghijk", NumericValue = 1, BooleanValue = true });
      testInstances.ForEach(instance => context.TestDummies.Add(instance));

      List<CustomMappedTestDummy> customMappedTestInstances = new List<CustomMappedTestDummy>();
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "aabcdef", NumericValue = 3, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "abcdefg", NumericValue = 3, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "aadefgh", NumericValue = 7, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "abefghi", NumericValue = 2, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "bbcdefg", NumericValue = 5, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "bbdefgh", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "bbefghi", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "bbfghij", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "acdefgh", NumericValue = 5, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "ccefghi", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "acfghij", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "ccghijk", NumericValue = 1, BooleanValue = true });
      customMappedTestInstances.ForEach(instance => context.CustomMappedTestDummies.Add(instance));
    }

    /// <summary>Seeds the database for the 'retrieve from empty' testcases.</summary>
    /// <param name="context">The context that gives access to the datasource.</param>
    private static void SeedRetrieveEmptyCategory(DatabaseRepositoryTestContext context) {
      /* Nothing to do */
    }

    /// <summary>Seeds the database for the 'storage' testcases.</summary>
    /// <param name="context">The context that gives access to the datasource.</param>
    private static void SeedStorageCategory(DatabaseRepositoryTestContext context) {
      List<TestDummy> testInstances = new List<TestDummy>();
      testInstances.Add(new TestDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false });
      testInstances.Add(new TestDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true });
      testInstances.ForEach(instance => context.TestDummies.Add(instance));

      List<TestSubDummy> subInstances = new List<TestSubDummy>();
      subInstances.Add(new TestSubDummy { TextValue = "Row1.A" });
      subInstances.Add(new TestSubDummy { TextValue = "Row1.B" });
      subInstances.Add(new TestSubDummy { TextValue = "Row1.C" });

      List<TestParentDummy> parentInstances = new List<TestParentDummy>();
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false, Children = subInstances });
      parentInstances.Add(new TestParentDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true });
      parentInstances.ForEach(instance => context.TestParentDummies.Add(instance));

      List<CustomMappedTestDummy> customMappedTestInstances = new List<CustomMappedTestDummy>();
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row1\"", NumericValue = 3, BooleanValue = false });
      customMappedTestInstances.Add(new CustomMappedTestDummy { TextValue = "\"Row2\"", NumericValue = 3, BooleanValue = true });
      customMappedTestInstances.ForEach(instance => context.CustomMappedTestDummies.Add(instance));
    }
  }
}
