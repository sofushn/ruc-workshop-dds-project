# Main

The structure of the repos is as follows:

- `docs`: contains all documentations files, including guides, protocol difinition and diagrams for the system
- `src`: contains all source code for the project
  - `lib`: shared library files across services
  - `svc`: independent services in the system
- `infra`: infrastructure files for deployment of resources, this is primaryly for files related to deployments in k8s