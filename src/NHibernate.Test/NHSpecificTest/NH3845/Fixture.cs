﻿using System.Linq;
using NHibernate.Linq;
using NHibernate.Test.NHSpecificTest.NH3845.Concrete;
using NHibernate.Test.NHSpecificTest.NH3845.Interfaces;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3845
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var entityA = new PropertyEntityA()
				{
					Name = "Name A",
					SerialNumber = 4321,
					SharedValue = "Some Value"
				};
				var entityB = new PropertyEntityB()
				{
					Name = "Name B",
					Description = "Some Description",
					SharedValue = "Another Value",
					AnotherString = "Another String"
				};
				var entityC = new PropertyEntityC()
				{
					Name = "Name C",
					Description = "Has Description",
					SharedValue = "Value",
					AnotherNumber = 42
				};

				var separateEntity = new SeparateEntity()
				{
					SeparateEntityValue = 5432
				};

				var mainEntity = new MainEntity()
				{
					Text = "Main Entity Text"
				};
				var secondMainEntity = new MainEntity()
				{
					Text = "Second Entity Text"
				};
				session.Save(mainEntity);
				session.Save(secondMainEntity);
				entityA.MainEntity = mainEntity;
				entityB.MainEntity = mainEntity;
				entityC.MainEntity = secondMainEntity;
				separateEntity.MainEntity = secondMainEntity;
				session.Save(entityA);
				session.Save(entityB);
				session.Save(entityC);
				session.Save(separateEntity);

				//mainEntity.Properties.Add(entityA);
				//mainEntity.Properties.Add(entityB);
				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void OfTypeWorksWithSingleEntityInterface()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result =
					entityQuery.Where(m => m.Properties.OfType<IPropertyEntityB>().Any()).ToList();
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void OfTypeWorksWithUnrelatedInterface()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result =
					entityQuery.Where(m => m.Properties.OfType<IHasDescription>().Any()).ToList();
				Assert.AreEqual(2, result.Count);
			}
		}

		[Test]
		public void CanQueryOfTypePropertyWithUnrelatedInterface()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result =
					entityQuery.Where(m => m.Properties.OfType<IHasDescription>().Any(d => d.Description == "Has Description"))
								.ToList();
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ImpossibleOfTypeReturnsNoResults()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result =
					entityQuery.Where(m => m.Properties.OfType<ISession>().Any()).ToList();
				Assert.IsEmpty(result);
			}
		}

		[Test]
		public void ImpossibleMappedOfTypeReturnsNoResults()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result =
					entityQuery.Where(m => m.Properties.OfType<ISeparateEntity>().Any(se => se.SeparateEntityValue == 5432)).ToList();
				Assert.IsEmpty(result);
			}
		}

		[Test]
		public void OfTypeAppliedToNonSubclassEntityStillWorks()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var entityQuery = session.Query<IMainEntity>();
				var result = entityQuery.Where(m => m.SeparateEntities.OfType<SeparateEntity>().Any()).ToList();
				Assert.AreEqual(1, result.Count);
			}
		}
	}
}
