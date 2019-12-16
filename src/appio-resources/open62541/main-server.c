#include "open62541.h"
#include <signal.h>
#include "constants.h"

UA_Boolean running = true;
static void stopHandler(int sig) {
    UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "received ctrl-c");
    running = false;
}

static void createTemperatureVariableNode(UA_Server* server) {
    char* nodeName            = "temperature";
    char* nodeLocale          = "en-US";

    UA_VariableAttributes attr      = UA_VariableAttributes_default;
    UA_Int32 temperatureValue       = 45;

    attr.displayName                = UA_LOCALIZEDTEXT(nodeLocale, nodeName);

    UA_Variant_setScalar(&attr.value, &temperatureValue, &UA_TYPES[UA_TYPES_INT32]);

    UA_NodeId temperatureNodeId     = UA_NODEID_STRING(1, nodeName);
    UA_NodeId parentNodeId          = UA_NODEID_NUMERIC(0, UA_NS0ID_OBJECTSFOLDER);
    UA_NodeId parentReferenceNodeId = UA_NODEID_NUMERIC(0, UA_NS0ID_ORGANIZES);
    UA_QualifiedName browseName     = UA_QUALIFIEDNAME(1, nodeName);
    UA_NodeId variableType          = UA_NODEID_NULL;

    UA_Server_addVariableNode(
                server,
                temperatureNodeId,
                parentNodeId,
                parentReferenceNodeId,
                browseName,
                variableType,
                attr,
                NULL,
                NULL);
}

int main(void) {
    signal(SIGINT, stopHandler);
    signal(SIGTERM, stopHandler);

	UA_Server *server = UA_Server_new();
	UA_ServerConfig *config = UA_Server_getConfig(server);
	UA_ServerConfig_setCustomHostname(config, ((const UA_String) UA_String_fromChars(SERVER_APP_HOSTNAME)));
	UA_ServerConfig_setMinimal(config, SERVER_APP_PORT, NULL);
    
	createTemperatureVariableNode(server);

	UA_StatusCode retval;
	if (loadInformationModels(server) == UA_STATUSCODE_GOOD && addCallbacks(server) == UA_STATUSCODE_GOOD)
	{
		retval = UA_Server_run(server, &running);
	}

    UA_Server_delete(server);
    return (int)retval;
}