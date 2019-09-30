/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System;
using Moq;
using NUnit.Framework;
using Appio.Resources.text.logging;

namespace Appio.ObjectModel.Tests
{
    public class CertificateGeneratorTests
    {
        private Mock<IFileSystem> _fileSystemMock;
        private Mock<ILoggerListener> _loggerListenerMock;
        private CertificateGenerator _objectUnderTest;
        
        const string TemplateConfig = @"[req]
prompt=no
default_bits=$KEY_SIZE
distinguished_name=distinguished_name
default_md=sha256

[v3]
basicConstraints=critical, CA:FALSE
nsComment='""OpenSSL Generated Certificate""'
subjectKeyIdentifier=hash
authorityKeyIdentifier=keyid, issuer

keyUsage=critical, digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment,keyCertSign
extendedKeyUsage=critical, serverAuth, clientAuth

[distinguished_name]
O=$ORG_NAME
CN=$COMMON_NAME";

        private static string GetExpectedConfig(uint keySize, string appName, string organization)
        {
	        return $@"[req]
prompt=no
default_bits={keySize}
distinguished_name=distinguished_name
default_md=sha256

[v3]
basicConstraints=critical, CA:FALSE
nsComment='""OpenSSL Generated Certificate""'
subjectKeyIdentifier=hash
authorityKeyIdentifier=keyid, issuer

keyUsage=critical, digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment,keyCertSign
extendedKeyUsage=critical, serverAuth, clientAuth

[distinguished_name]
O={organization}
CN={appName}";
        }

        [SetUp]
        public void Setup()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _objectUnderTest = new CertificateGenerator(_fileSystemMock.Object);
            
            
            _loggerListenerMock = new Mock<ILoggerListener>();
            AppioLogger.RegisterListener(_loggerListenerMock.Object);
        }

        [TearDown]
        public void Cleanup()
        {
	        AppioLogger.RemoveListener(_loggerListenerMock.Object);
        }
        
        [TestCase("MyApp", 1024u, 365u, "MyOrg")]
        [TestCase("MyOtherApp", 2048u, 30u, "MyOtherOrg")]
        public void GenerateCertificate(string appName, uint keySize, uint days, string organization)
        {
	        // Arrange
	        var projectDirectory = appName;
	        const string certificateFolder = "certificate-folder";
            const string certificateConfigPath = "certificate-config-file";
            const string privateKeyPEM = "private-key-pem";

            _fileSystemMock.Setup(x => x.CombinePaths(projectDirectory, Constants.DirectoryName.Certificates)).Returns(certificateFolder);
			_fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig)).Returns(certificateConfigPath);
			_fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.PrivateKeyPEM)).Returns(privateKeyPEM);
			_fileSystemMock.Setup(x => x.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName)).Returns(TemplateConfig);
			
			// Act
			_objectUnderTest.Generate(appName, string.Empty, keySize, days, organization);
			
			// Assert
            _fileSystemMock.Verify(fs => fs.CreateDirectory(certificateFolder), Times.Once);
            _fileSystemMock.Verify(fs => fs.CreateFile(certificateConfigPath, GetExpectedConfig(keySize, appName, organization)), Times.Once);
            _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSL, string.Empty, days)), Times.Once);
            _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, Constants.FileName.PrivateKeyPEM, Constants.FileName.PrivateKeyDER)), Times.Once);
            _fileSystemMock.Verify(fs => fs.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName), Times.Once);
            _fileSystemMock.Verify(fs => fs.DeleteFile(privateKeyPEM), Times.Once);
            _loggerListenerMock.Verify(listener => listener.Info(string.Format(LoggingText.CertificateGeneratorSuccess, projectDirectory)));
        }

        [TestCase("OneApp")]
        [TestCase("AnotherApp")]
        public void GenerateDefaultCertificate(string appName)
        {
	        // Arrange
	        var projectDirectory = appName;
	        const string certificateFolder = "certificate-folder";
	        const string certificateConfigPath = "certificate-config-file";
			
	        _fileSystemMock.Setup(x => x.CombinePaths(projectDirectory, Constants.DirectoryName.Certificates)).Returns(certificateFolder);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig)).Returns(certificateConfigPath);
	        _fileSystemMock.Setup(x => x.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName)).Returns(TemplateConfig);
	        
	        _objectUnderTest.Generate(appName);

	        // Assert
	        _fileSystemMock.Verify(fs => fs.CreateDirectory(certificateFolder), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CreateFile(certificateConfigPath, GetExpectedConfig(Constants.ExternalExecutableArguments.OpenSSLDefaultKeySize, appName, Constants.ExternalExecutableArguments.OpenSSLDefaultOrganization)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSL, string.Empty, Constants.ExternalExecutableArguments.OpenSSLDefaultDays)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, Constants.FileName.PrivateKeyPEM, Constants.FileName.PrivateKeyDER)), Times.Once);
	        _loggerListenerMock.Verify(listener => listener.Info(string.Format(LoggingText.CertificateGeneratorSuccess, projectDirectory)));
        }
        
        [TestCase("OneApp")]
        [TestCase("AnotherApp")]
        public void GenerateCertificateWithPrefix(string appName)
        {
	        // Arrange
	        const string prefix = "prefix";
	        var projectDirectory = appName;
	        const string certificateFolder = "certificate-folder";
	        const string certificateConfigPath = "certificate-config-file";
	        const string privateKeyPEM = "private-key-pem";
			
	        _fileSystemMock.Setup(x => x.CombinePaths(projectDirectory, Constants.DirectoryName.Certificates)).Returns(certificateFolder);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig)).Returns(certificateConfigPath);
	        _fileSystemMock.Setup(x => x.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName)).Returns(TemplateConfig);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, prefix + "_" + Constants.FileName.PrivateKeyPEM)).Returns(privateKeyPEM);
	        
	        _objectUnderTest.Generate(appName, prefix);

	        // Assert
	        _fileSystemMock.Verify(fs => fs.CreateFile(certificateConfigPath, GetExpectedConfig(Constants.ExternalExecutableArguments.OpenSSLDefaultKeySize, appName, Constants.ExternalExecutableArguments.OpenSSLDefaultOrganization)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSL, prefix + "_", Constants.ExternalExecutableArguments.OpenSSLDefaultDays)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, prefix + "_" + Constants.FileName.PrivateKeyPEM, prefix + "_" + Constants.FileName.PrivateKeyDER)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName), Times.Once);
	        _fileSystemMock.Verify(fs => fs.DeleteFile(privateKeyPEM), Times.Once);
	        _loggerListenerMock.Verify(listener => listener.Info(string.Format(LoggingText.CertificateGeneratorSuccess, projectDirectory + "/" + prefix)));
        }

        [Test]
        public void FailWithIllegalDirectory()
        {
	        // Arrange
	        const string appName = "nonexistent-directory";
	        const string projectDirectory = appName;
	        const string certificateFolder = "certificate-folder";
	        const string certificateConfigPath = "certificate-config-file";

	        _fileSystemMock.Setup(x => x.CombinePaths(projectDirectory, Constants.DirectoryName.Certificates)).Returns(certificateFolder);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig)).Returns(certificateConfigPath);
	        _fileSystemMock.Setup(x => x.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName)).Returns(TemplateConfig);
	        _fileSystemMock.Setup(x => x.CreateFile(certificateConfigPath, It.IsAny<string>())).Throws<Exception>();
	        
	        _objectUnderTest.Generate(appName);

	        // Assert
	        _fileSystemMock.Verify(fs => fs.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CreateFile(certificateConfigPath, GetExpectedConfig(Constants.ExternalExecutableArguments.OpenSSLDefaultKeySize, appName, Constants.ExternalExecutableArguments.OpenSSLDefaultOrganization)), Times.Once);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSL, string.Empty, Constants.ExternalExecutableArguments.OpenSSLDefaultDays)), Times.Never);
	        _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, Constants.FileName.PrivateKeyPEM, Constants.FileName.PrivateKeyDER)), Times.Never);
	        _loggerListenerMock.Verify(listener => listener.Warn(string.Format(LoggingText.CertificateGeneratorFailureNonexistentDirectory, projectDirectory)));
        }
        
        [Test]
        public void FailWhenOpenSSLNotWorking()
        {
	        // Arrange
	        const string projectDirectory = "MyApp";
	        const string certificateFolder = "certificate-folder";
	        const string certificateConfigPath = "certificate-config-file";
	        const string privateKeyPEM = "private-key-pem";

	        _fileSystemMock.Setup(x => x.CombinePaths(projectDirectory, Constants.DirectoryName.Certificates)).Returns(certificateFolder);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig)).Returns(certificateConfigPath);
	        _fileSystemMock.Setup(x => x.CombinePaths(certificateFolder, Constants.FileName.PrivateKeyPEM)).Returns(privateKeyPEM);
	        _fileSystemMock.Setup(x => x.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName)).Returns(TemplateConfig);
	        _fileSystemMock.Setup(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, It.IsAny<string>())).Throws<Exception>();
	        
	        _objectUnderTest.Generate("MyApp");

	        // Assert
	        _loggerListenerMock.Verify(listener => listener.Warn(LoggingText.CertificateGeneratorFailureOpenSSLError));
        }
    }
}