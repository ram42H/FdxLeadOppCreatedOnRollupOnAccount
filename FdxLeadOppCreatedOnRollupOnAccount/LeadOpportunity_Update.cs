using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FdxLeadOppCreatedOnRollupOnAccount
{
    public class LeadOpportunity_Update : IPlugin
    {
        int step;
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins....
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //Obtain execution contest from the service provider....
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            step = 0;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity triggeredEntity = (Entity)context.InputParameters["Target"];

                try
                {
                    step = 1;
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    //Get current user information....
                    step = 2;
                    WhoAmIResponse response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

                    step = 3;
                    switch (triggeredEntity.LogicalName)
                    {
                        case "lead":
                            step = 4;
                            Entity leadEntity = new Entity();
                            leadEntity = service.Retrieve("lead", triggeredEntity.Id, new ColumnSet(true));
                            this.updateAccount(leadEntity, service);
                            break;
                        case "opportunity":
                            step = 5;
                            Entity opportunityEntity = new Entity();
                            opportunityEntity = service.Retrieve("opportunity", triggeredEntity.Id, new ColumnSet(true));
                            this.updateAccount(opportunityEntity, service);
                            break;
                        default:
                            return;
                    }

                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("An error occurred in the LeadOpportunity_Update plug-in at Step {0}.", step), ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("LeadOpportunity_Update: step {0}, {1}", step, ex.ToString());
                    throw;
                }
            }
        }
        private void updateAccount(Entity _triggeredEntity, IOrganizationService _service)
        {
            step = 6;
            if (_triggeredEntity.Attributes.Contains("parentaccountid") && _triggeredEntity.Attributes.Contains("createdon"))
            {
                step = 7;
                Entity readAccount = new Entity();
                Entity updateAccount = new Entity();
                DateTime entityCreatedOnDate = (DateTime)_triggeredEntity.Attributes["createdon"];

                step = 8;
                readAccount = _service.Retrieve("account", ((EntityReference)_triggeredEntity.Attributes["parentaccountid"]).Id, new ColumnSet(true));

                if (readAccount.Attributes.Contains("fdx_lastleadopportunitycreatedon"))
                {
                    step = 9;
                    DateTime accountlastleadopportunitycreatedon = (DateTime)readAccount.Attributes["fdx_lastleadopportunitycreatedon"];

                    if (DateTime.Compare(accountlastleadopportunitycreatedon, entityCreatedOnDate) < 0)
                    {
                        step = 10;
                        updateAccount = new Entity
                        {
                            LogicalName = "account",
                            Id = readAccount.Id
                        };

                        step = 11;
                        updateAccount["fdx_lastleadopportunitycreatedon"] = entityCreatedOnDate;

                        step = 12;
                        _service.Update(updateAccount);
                    }
                }
                else
                {
                    step = 13;
                    updateAccount = new Entity
                    {
                        LogicalName = "account",
                        Id = readAccount.Id
                    };

                    step = 14;
                    updateAccount["fdx_lastleadopportunitycreatedon"] = entityCreatedOnDate;

                    step = 15;
                    _service.Update(updateAccount);
                }
            }
        }
    }
}
