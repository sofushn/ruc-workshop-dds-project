# Docker & Kubernetes Cheat Sheet

This cheat sheet covers basic commands for managing Docker containers/images and Kubernetes resources.

**(Remember:** Replace placeholders like `<image_name>`, `<tag>`, `<container_name>`, `<resource_type>`, `<resource_name>`, `<pod_name>`, `<namespace_name>`, and `my-app.yaml` with your actual values.)

## **Docker Commands**

### Managing Images

- **Build an image from a Dockerfile:**
  - `docker build -t <image_name>:<tag> .`
  - *Purpose:* Creates a Docker image using instructions in a `Dockerfile` located in the current directory (`.`). The `-t` flag tags the image with a name and optional tag.
  - *Example:* `docker build -t myapp:v1 .`
- **List local images:**
  - `docker images`
  - *Purpose:* Shows all Docker images stored on your local machine.
- **Tag an existing image:**
  - `docker tag <source_image>:<tag> <target_image>:<tag>`
  - *Purpose:* Creates an additional tag (alias) for an existing image. Often used before pushing to a registry.
  - *Example:* `docker tag myapp:v1 myregistry/myapp:latest`
- **Remove an image:**
  - `docker rmi <image_name>:<tag>` or `docker rmi <image_id>`
  - *Purpose:* Deletes an image from your local machine. You might need to remove containers using the image first.
  - *Example:* `docker rmi myapp:v1`

### Managing Containers

- **Run a container from an image:**
  - `docker run [OPTIONS] <image_name>:<tag>`
  - *Purpose:* Creates and starts a new container based on the specified image.
  - *Common Options:*
    - `-d`: Run in detached mode (background).
    - `-p <host_port>:<container_port>`: Map a port from your host to the container.
    - `--name <container_name>`: Assign a specific name to the container.
  - *Example:* `docker run -d -p 8080:80 --name webserver nginx:latest`
- **List running containers:**
  - `docker ps`
  - *Purpose:* Shows currently running containers.
  - `docker ps -a`: Shows all containers (running and stopped).
- **Remove a container:**
  - `docker rm <container_name>` or `docker rm <container_id>`
  - *Purpose:* Deletes a stopped container.
  - `docker rm -f <container_name>`: Force removes a running container (can be used instead of stopping the pod beforehand with `docker stop <container_id>`).
  - *Example:* `docker rm webserver`


## **Kubernetes (kubectl) Commands**

### Managing Resources (Declarative - Using Files)

- **Apply configuration from a file:**
  - `kubectl apply -f <filename.yaml>`
  - *Purpose:* Creates or updates Kubernetes resources defined in a YAML or JSON file. This is the preferred way to manage resources.
  - *Example:* `kubectl apply -f my-deployment.yaml`
- **Delete resources defined in a file:**
  - `kubectl delete -f <filename.yaml>`
  - *Purpose:* Deletes the Kubernetes resources that were created from the specified file.
  - *Example:* `kubectl delete -f my-deployment.yaml`

### Managing Resources (Imperative - Direct Commands)

- **Delete a specific resource by name:**
  - `kubectl delete <resource_type> <resource_name>`
  - *Purpose:* Directly deletes a specific resource (e.g., a Pod, Deployment, Service). Use the `-n <namespace_name>` flag if not in the default/current namespace.
  - *Example:* `kubectl delete deployment my-app-deployment`
  - *Example:* `kubectl delete pod my-pod-12345 -n dev`
- **Scale a resource (e.g., Deployment):**
  - `kubectl scale <resource_type>/<resource_name> --replicas=<count>`
  - *Purpose:* Changes the number of running Pods managed by a resource like a Deployment. Use the `-n <namespace_name>` flag if needed.
  - *Example:* `kubectl scale deployment/my-app-deployment --replicas=3`

### Inspecting & Interacting

- **Get (list) resources:**
  - `kubectl get <resource_type>`
  - *Purpose:* Lists resources of a specific type in the current namespace.
  - *Common Options:*
    - `<resource_type> <resource_name>`: Get a specific resource.
    - `-n <namespace_name>`: Specify a namespace to query *instead* of the current one.
    - `-o wide`: Show more details (like Node IP, Pod IP).
    - `all`: List common resource types (Pods, Services, Deployments, etc.).
  - *Example:* `kubectl get pods`
  - *Example:* `kubectl get deployments -n production`
  - *Example:* `kubectl get service my-app-service -o wide`
- **Describe a resource:**
  - `kubectl describe <resource_type> <resource_name>`
  - *Purpose:* Shows detailed information about a specific resource, including its status, configuration, recent events, and potential issues. Very useful for troubleshooting. Use the `-n <namespace_name>` flag if needed.
  - *Example:* `kubectl describe pod my-app-pod-xyz12`
  - *Example:* `kubectl describe deployment my-app-deployment -n dev`
- **Get logs from a Pod:**
  - `kubectl logs <pod_name> [OPTIONS]`
  - *Purpose:* Displays logs from a container within a specified Pod. Essential for debugging applications. Use the `-n <namespace_name>` flag if needed.
  - *Common Options:*
    - `-c <container_name>`: Specify the container name if the Pod has multiple containers.
    - `-f` or `--follow`: Stream logs in real-time.
    - `-p` or `--previous`: Show logs from a previous (terminated) instance of the container.
    - `--tail=<number>`: Show only the last N lines of logs.
  - *Example:* `kubectl logs my-app-pod-xyz12`
  - *Example:* `kubectl logs my-app-pod-xyz12 -c sidecar-container`
  - *Example:* `kubectl logs -f my-app-pod-xyz12 -n production`
  - *Example:* `kubectl logs --tail=100 my-app-pod-xyz12`
- **Port-forward to a resource:**
  - `kubectl port-forward <resource_type>/<resource_name> <local_port>:<resource_port>`
  - *Purpose:* Forwards traffic from a port on your local machine to a port on a Pod or Service inside the cluster. Useful for debugging or accessing internal services. Use the `-n <namespace_name>` flag if needed.
  - *Example (Pod):* `kubectl port-forward pod/my-app-pod-xyz12 8080:80`
  - *Example (Service):* `kubectl port-forward svc/my-app-service 9000:80 -n staging`


## **Helpful Utility: `kubens` (Optional)**

- **(Note:** `kubens` is a popular *third-party tool*, not part of standard `kubectl`. It needs to be installed separately - often found with `kubectx` from `ahmetb` on GitHub.)
- **Purpose:** Quickly view and switch your *current default namespace* for subsequent `kubectl` commands, making it easier than manually setting the context each time.
- **Common Commands:**
  - `kubens` : Lists all available namespaces and highlights the currently active one.
  - `kubens <namespace_name>` : Sets the default namespace for your current context.
  - `kubens -` : Switches back to the previously used namespace.
- *Example:*
  - Run `kubens dev` to switch to the `dev` namespace.
  - Now, running `kubectl get pods` will show pods in the `dev` namespace without needing `-n dev`.
  - Run `kubens prod` to switch to the `prod` namespace.


### **Common Kubernetes Resources**

- **Pod:**
  - The smallest deployable unit in Kubernetes.
  - Contains one or more containers that share storage and network.
  - Usually managed by higher-level resources like Deployments, not created directly often.
- **Deployment:**
  - Manages a set of identical Pods (replicas).
  - Ensures the desired number of Pods are running.
  - Handles updates (rolling updates) and rollbacks.
- **Namespace:**
  - A way to divide cluster resources between multiple users or teams (virtual cluster).
  - Provides scope for names; resource names must be unique within a namespace. (`kubens` helps manage these).
- **Service:**
  - Provides a stable IP address and DNS name for accessing a set of Pods.
  - Acts as a load balancer for the Pods it targets.
  - Enables communication between different parts of your application or external access.
- **Ingress:**
  - Manages external access to Services in the cluster, typically HTTP/HTTPS.
  - Provides routing rules, SSL termination, and name-based virtual hosting.
  - Requires an Ingress controller to be running in the cluster. (k3d uses Traefik)
- **Node:**
  - A worker machine (virtual or physical) in the Kubernetes cluster.
  - Runs Pods and is managed by the Kubernetes control plane.
