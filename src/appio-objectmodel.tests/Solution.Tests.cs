﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using Newtonsoft.Json;
using NUnit.Framework;
using System.Linq;

namespace Appio.ObjectModel.Tests
{
    public class SolutionShould
    {
        private ISolution _solution;

        [SetUp]
        public void SetupTest()
        {
            _solution = new Solution();   
        }

        [TearDown]
        public void CleanUpTest()
        {
        }

        [Test]
        public void NotContainAnyProjectWhenCreated()
        {
            // Arrange

            // Act
            
            // Assert
            Assert.IsNotNull(_solution);
            Assert.AreEqual(0, _solution.Projects.Count());
        }

        [Test]
        public void BeSerializableToJson()
        {
            // Arrange

            // Act
            var solutionAsJson = JsonConvert.SerializeObject(_solution, Formatting.Indented);
            
            // Assert
            Assert.IsNotNull(solutionAsJson);
            Assert.AreNotEqual(string.Empty, solutionAsJson);
        }

        [Test]
        public void ContainOneProjectAndBeSerializableToJson()
        {
            // Arrange
            var projectName = "mvpSmartPump";
            var projectPath = "mvpSmartPump/mvpSmartPump.appioproj";
            _solution.Projects.Add(new OpcuaappReference() { Name = projectName, Path = projectPath });
            
            // Act
            var solutionAsJson = JsonConvert.SerializeObject(_solution, Formatting.Indented);

            // Assert
            Assert.IsNotNull(solutionAsJson);
            Assert.AreNotEqual(string.Empty, solutionAsJson);
            Assert.IsTrue(solutionAsJson.Contains(projectName)); // don't care where
            Assert.IsTrue(solutionAsJson.Contains(projectPath)); // don't care where
        }

        [Test]
        public void BeDeSerializableFromJson()
        {
            // Arrange
            var solutionAsJson = "" +
                "{" +
                    "\"projects\": []" +
                "}";

            // Act
            ISolution solution = JsonConvert.DeserializeObject<Solution>(solutionAsJson);

            // Assert
            Assert.IsNotNull(solution);
            Assert.AreEqual(0, solution.Projects.Count());
        }

        [Test]
        public void ContainOneProjectAndBedeserializableFromJson()
        {
            // Arrange
            var solutionAsJson = "{ \"projects\":[{\"name\":\"mvpSmartPump\",\"path\":\"mvpSmartPump/mvpSmartPump.appioproj\"}]}";           

            // Act
            ISolution solution = JsonConvert.DeserializeObject<Solution>(solutionAsJson);

            // Assert
            Assert.IsNotNull(solution);
            Assert.AreEqual(1, solution.Projects.Count());
        }

        [Test]
        public void ContainTwoProjectAndBeDeSerializableFromJson()
        {
            // Arrange          
            var solutionAsJson = "{ \"projects\":[{\"name\":\"mvpSmartPump\",\"path\":\"mvpSmartPump/mvpSmartPump.appioproj\"}," +
                "{\"name\":\"mvpSmartLiterSensor\",\"path\":\"mvpSmartLiterSensor/mvpSmartLiterSensor.appioproj\"}]}";
            
            ISolution solution = JsonConvert.DeserializeObject<Solution>(solutionAsJson);

            // Assert
            Assert.IsNotNull(solution);
            Assert.AreEqual(2, solution.Projects.Count());
        }
    }
}