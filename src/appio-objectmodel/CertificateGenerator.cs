/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System;
using System.Text;
using Appio.ObjectModel;
using Appio.Resources.text.logging;

namespace Appio.ObjectModel
{
    public class CertificateGenerator : AbstractCertificateGenerator
    {
        private readonly IFileSystem _fileSystem;

        public CertificateGenerator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override void Generate(string appName, string filePrefix, uint keySize, uint days, string organization)
        {
            var openSSLConfigBuilder =
                new StringBuilder(_fileSystem.LoadTemplateFile(Resources.Resources.OpenSSLConfigTemplateFileName));
            openSSLConfigBuilder.Replace("$KEY_SIZE", keySize.ToString());
            openSSLConfigBuilder.Replace("$ORG_NAME", organization);
            openSSLConfigBuilder.Replace("$COMMON_NAME", appName);

            var certificateFolder = _fileSystem.CombinePaths(appName, Constants.DirectoryName.Certificates);
            var certificateConfigPath = _fileSystem.CombinePaths(certificateFolder, Constants.FileName.CertificateConfig);
            try
            {
                _fileSystem.CreateDirectory(certificateFolder);
                _fileSystem.CreateFile(certificateConfigPath, openSSLConfigBuilder.ToString());
            }
            catch (Exception)
            {
                AppioLogger.Warn(string.Format(LoggingText.CertificateGeneratorFailureNonexistentDirectory, appName));
                return;
            }

            var separator = filePrefix == string.Empty ? "" : "_";

            try
            {
                _fileSystem.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSL, filePrefix + separator, days));
                _fileSystem.CallExecutable(Constants.ExecutableName.OpenSSL, certificateFolder, string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, filePrefix + separator + Constants.FileName.PrivateKeyPEM, filePrefix + separator + Constants.FileName.PrivateKeyDER));
            }
            catch (Exception)
            {
                AppioLogger.Warn(LoggingText.CertificateGeneratorFailureOpenSSLError);
            }
            
            _fileSystem.DeleteFile(_fileSystem.CombinePaths(certificateFolder, filePrefix + separator + Constants.FileName.PrivateKeyPEM));
            AppioLogger.Info(string.Format(LoggingText.CertificateGeneratorSuccess, filePrefix == string.Empty ? appName : appName + "/" + filePrefix));
        }
    }
}