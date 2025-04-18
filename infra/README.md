# Infrastructure

This project is being built to run in k8s, this allows the project to test different horizontal scaling techniques.

To simplify testing of scaling configurations we deploy all services in a kubernetes (k8s) cluster where we can increase and decrease the number of replicas per service much easier.

## Deploying to k8s

> [!IMPORTANT]
> Requires a [Docker](https://www.docker.com/) as well as [k3d](https://k3d.io/stable/) installed locally.

### Run cluster locally

1. Setup cluster by navigating to `infra` folder
2. Run `k3d cluster create --config k3dcluster.yaml` (Make sure that docker is running in the background)
3. Deploy each k8s configuration resource individually with `kubectl apply -f <path to .yaml file>`

### Remote VM

Comming soon...
