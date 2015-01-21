﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery;
using Microsoft.Azure.Portal.RecoveryServices.Models.Common;
using Microsoft.WindowsAzure.Management.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices
{
    /// <summary>
    /// Adds Azure Site Recovery Protection Profile settings to a Protection Container.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureSiteRecoveryProtectionProfileAssociationJob", DefaultParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
    [OutputType(typeof(ASRJob))]
    public class StartAzureSiteRecoveryProtectionProfileJob : RecoveryServicesCmdletBase
    {
        /// <summary>
        /// Job response.
        /// </summary>
        private JobResponse jobResponse = null;

        #region Parameters

        /// <summary>
        /// Gets or sets Protection Container to be applied the Protection Profile settings on.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToAzure, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ASRProtectionContainer PrimaryProtectionContainer { get; set; }

        /// <summary>
        /// Gets or sets Protection Container to be applied the Protection Profile settings on.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToAzure, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ASRProtectionContainer RecoveryProtectionContainer { get; set; }

        /// <summary>
        /// Gets or sets Protection Profile object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.Default, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ASRProtectionProfile ProtectionProfile { get; set; }

        #endregion Parameters

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            try
            {
                switch (this.ParameterSetName)
                {
                    case ASRParameterSets.EnterpriseToAzure:
                        this.EnterpriseToEnterpriseAssociation();
                        break;
                    case ASRParameterSets.EnterpriseToEnterprise:
                        this.EnterpriseToAzureAssociation();
                        break;
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }

        /// <summary>
        /// Handles interrupts.
        /// </summary>
        protected override void StopProcessing()
        {
            // Ctrl + C and etc
            base.StopProcessing();
            this.StopProcessingFlag = true;
        }

        /// <summary>
        /// Associates protection profile with one enterprise based and an Azure protection container
        /// </summary>
        private void EnterpriseToAzureAssociation()
        {
            HyperVReplicaAzureProtectionProfileInput hyperVReplicaAzureProtectionProfileInput
                    = new HyperVReplicaAzureProtectionProfileInput()
                    {
                        AppConsistencyFreq = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ApplicationConsistentSnapshotFrequencyInHours,
                        ReplicationInterval = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ReplicationFrequencyInSeconds,
                        OnlineIrStartTime = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ReplicationStartTime
                    };

            var storageAccount = new CustomerStorageAccount();
            storageAccount.StorageAccountName = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.RecoveryAzureStorageAccountName;
            storageAccount.SubscriptionId = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.RecoveryAzureSubscription;
            hyperVReplicaAzureProtectionProfileInput.StorageAccounts.Add(storageAccount);

            CreateProtectionProfileInput createProtectionProfileInput =
                new CreateProtectionProfileInput(
                    this.ProtectionProfile.Name,
                    this.ProtectionProfile.ReplicationProvider,
                    DataContractUtils<HyperVReplicaAzureProtectionProfileInput>.Serialize(hyperVReplicaAzureProtectionProfileInput));

            ProtectionProfileAssociationInput protectionProfileAssociationInput =
                new ProtectionProfileAssociationInput(
                    this.PrimaryProtectionContainer.ID,
                    this.RecoveryProtectionContainer.ID);

            CreateAndAssociateProtectionProfileInput createAndAssociateProtectionProfileInput =
                new CreateAndAssociateProtectionProfileInput(
                    createProtectionProfileInput,
                    protectionProfileAssociationInput);

            this.jobResponse = RecoveryServicesClient.StartCreateAndAssociateAzureSiteRecoveryProtectionProfileJob(
                createAndAssociateProtectionProfileInput);

            this.WriteJob(this.jobResponse.Job);
        }

        /// <summary>
        /// Associates protection profile with enterprise based protection containers
        /// </summary>
        private void EnterpriseToEnterpriseAssociation()
        {
            HyperVReplicaAzureProtectionProfileInput hyperVReplicaAzureProtectionProfileInput
                    = new HyperVReplicaAzureProtectionProfileInput()
                    {
                        AppConsistencyFreq = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ApplicationConsistentSnapshotFrequencyInHours,
                        ReplicationInterval = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ReplicationFrequencyInSeconds,
                        OnlineIrStartTime = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.ReplicationStartTime
                    };

            var storageAccount = new CustomerStorageAccount();
            storageAccount.StorageAccountName = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.RecoveryAzureStorageAccountName;
            storageAccount.SubscriptionId = this.ProtectionProfile.HyperVReplicaAzureProviderSettingsObject.RecoveryAzureSubscription;
            hyperVReplicaAzureProtectionProfileInput.StorageAccounts.Add(storageAccount);

            CreateProtectionProfileInput createProtectionProfileInput =
                new CreateProtectionProfileInput(
                    this.ProtectionProfile.Name,
                    this.ProtectionProfile.ReplicationProvider,
                    DataContractUtils<HyperVReplicaAzureProtectionProfileInput>.Serialize(hyperVReplicaAzureProtectionProfileInput));

            ProtectionProfileAssociationInput protectionProfileAssociationInput =
                new ProtectionProfileAssociationInput(
                    this.PrimaryProtectionContainer.ID,
                    this.RecoveryProtectionContainer.ID);

            CreateAndAssociateProtectionProfileInput createAndAssociateProtectionProfileInput =
                new CreateAndAssociateProtectionProfileInput(
                    createProtectionProfileInput,
                    protectionProfileAssociationInput);

            this.jobResponse = RecoveryServicesClient.StartCreateAndAssociateAzureSiteRecoveryProtectionProfileJob(
                createAndAssociateProtectionProfileInput);

            this.WriteJob(this.jobResponse.Job);
        }

        /// <summary>
        /// Writes Job
        /// </summary>
        /// <param name="job">Job object</param>
        private void WriteJob(Microsoft.WindowsAzure.Management.SiteRecovery.Models.Job job)
        {
            this.WriteObject(new ASRJob(job));
        }
    }
}
