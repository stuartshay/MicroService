# HELM Chart For OpenFaaS

## Prerequsites:
- Kubernetes or minikube installed
- Helm installed and tiller configured correctly
- faas-cli installed


## Deploy OpenFaaS

**Note:** If your cluster is not configured with role-based access control, then pass `--set rbac=false`.

---
### Installing the chart

Before installing the chart create namespaces:

Two namespaces are recomended,
- one for the OpenFaaS core services
- one for the functions:

The two namespaces are placed in openfaas folder.

Use the command below to create them:
```sh
kubectl create -f namespaces.yaml
```

#### Now deploy OpenFaaS from the helm chart repo:

```sh
helm install -n openfaas --namespace openfaas .
```
The above command will install full helm chart for Openfaas

**Note:**
- You would need a username and password to login.
- You can change the username and password from values.yaml under secrets.
- The secret is encoded in base64 .




### Verify the installation

Fetch your public IP or NodePort via
```sh
kubectl get svc -n openfaas gateway-external -o wide
```

Export openfaas url:
```sh
export OPENFAAS_URL=http://127.0.0.1:31112
```

If using a remote cluster, you can port-forward the gateway to your local machine:

```sh
kubectl port-forward -n openfaas svc/gateway 31112:8080
```

Now log in with the CLI and check connectivity:

```sh
faas-cli login -u admin --password <enter your password>

Once you are logged in you will be able to use the following commands:

faas-cli list    (to list the functions)
faas-cli version (to check openfaas version)
faas-cli build   (to build docker image from supported language tpes)
faas-cli new     (to  create new function via template directory)
```


## Exposing services

### NodePorts

By default a NodePort will be created for the API Gateway.


### Deploy with an IngressController

In order to make use of automatic ingress settings you will need an IngressController in your cluster such as Traefik or Nginx.
- Add `--set ingress.enabled` to enable ingress pass `--set ingress.enabled=true` when running the installation via `helm`.

By default services will be exposed with following hostnames (can be changed, see values.yaml for details):

* `gateway.openfaas.local`



## Configuration

Additional OpenFaaS options in `values.yaml`.

| Parameter               | Description                           | Default                                                    |
| ----------------------- | ----------------------------------    | ---------------------------------------------------------- |
| `functionNamespace` | Functions namespace, preferred `openfaas-fn` | `default` |
| `async` | Deploys NATS | `true` |
| `exposeServices` | Expose `NodePorts/LoadBalancer`  | `true` |
| `serviceType` | Type of external service to use `NodePort/LoadBalancer` | `NodePort` |
| `basic_auth` | Enable basic authentication on the Gateway | `true` |
| `rbac` | Enable RBAC | `true` |
| `httpProbe` | Setting to true will use HTTP for readiness and liveness probe on the OpenFaaS system Pods (compatible with Istio >= 1.1.5) | `true` |
| `securityContext` | Deploy with a `securityContext` set, this can be disabled for use with Istio sidecar injection | `true` |
| `openfaasImagePullPolicy` | Image pull policy for openfaas components, can change to `IfNotPresent` in offline env | `Always` |
| `kubernetesDNSDomain` | Domain name of the Kubernetes cluster | `cluster.local` |
| `operator.create` | Use the OpenFaaS operator CRD controller, default uses faas-netes as the Kubernetes controller | `false` |
| `operator.createCRD` | Create the CRD for OpenFaaS Function definition | `true` |
| `ingress.enabled` | Create ingress resources | `false` |
| `faasnetes.httpProbe` | Use a httpProbe instead of exec | `false` |
| `faasnetes.readTimeout` | Queue worker read timeout | `60s` |
| `faasnetes.writeTimeout` | Queue worker write timeout | `60s` |
| `faasnetes.imagePullPolicy` | Image pull policy for deployed functions | `Always` |
| `faasnetes.setNonRootUser` | Force all function containers to run with user id `12000` | `false` |
| `gateway.replicas` | Replicas of the gateway, pick more than `1` for HA | `1` |
| `gateway.readTimeout` | Queue worker read timeout | `65s` |
| `gateway.writeTimeout` | Queue worker write timeout | `65s` |
| `gateway.upstreamTimeout` | Maximum duration of upstream function call, should be lower than `readTimeout`/`writeTimeout` | `60s` |
| `gateway.scaleFromZero` | Enables an intercepting proxy which will scale any function from 0 replicas to the desired amount | `true` |
| `gateway.maxIdleConns` | Set max idle connections from gateway to functions | `1024` |
| `gateway.maxIdleConnsPerHost` | Set max idle connections from gateway to functions per host | `1024` |
| `queueWorker.replicas` | Replicas of the queue-worker, pick more than `1` for HA | `1` |
| `queueWorker.ackWait` | Max duration of any async task/request | `60s` |
| `nats.enableMonitoring` | Enable the NATS monitoring endpoints on port `8222` | `false` |
| `faasIdler.create` | Create the faasIdler component | `true` |
| `faasIdler.inactivityDuration` | Duration after which faas-idler will scale function down to 0 | `15m` |
| `faasIdler.reconcileInterval` | The time between each of reconciliation | `1m` |
| `faasIdler.dryRun` | When set to false the OpenFaaS API will be called to scale down idle functions, by default this is set to only print in the logs. | `true` |
| `prometheus.create` | Create the Prometheus component | `true` |
| `alertmanager.create` | Create the AlertManager component | `true` |


Specify each parameter using the `--set key=value[,key=value]` argument to `helm install`.
See values.yaml for detailed configuration.

## Removing the OpenFaaS

Use this command to delete OpenFaaS

```sh
helm delete --purge openfaas
```

Use this to remove all other associated objects:

```sh
kubectl delete namespace openfaas openfaas-fn
```
