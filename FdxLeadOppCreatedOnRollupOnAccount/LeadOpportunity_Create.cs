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
    public class LeadOpportunity_Create : IPlugin
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
                        case "opportunity":
                            step = 4;
                            this.updateAccount(triggeredEntity, service);
                            break;
                        default:
                            return;
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("An error occurred in the LeadOpportunity_Create plug-in at Step {0}.", step), ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("LeadOpportunity_Create: step {0}, {1}", step, ex.ToString());
                    throw;
                }
            }
        }
        private void updateAccount(Entity _triggeredEntity, IOrganizationService _service)
        {
            step = 6;
            if (_triggeredEntity.Attributes.Contains("parentaccountid"))
            {
                step = 7;
                Entity updateAccount = new Entity();

                step = 8;
                updateAccount = new Entity
                {
                    LogicalName = "account",
                    Id = ((EntityReference)_triggeredEntity.Attributes["parentaccountid"]).Id
                };

                step = 9;
                updateAccount["fdx_lastleadopportunitycreatedon"] = DateTime.UtcNow;

                step = 10;
                _service.Update(updateAccount);
            }
        }
    }
}
