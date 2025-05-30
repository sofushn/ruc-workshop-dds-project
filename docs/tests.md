# This is nodes for how tests are run

```
k3d cluster delete street-view
```

## Apply all non-replicated services 

```
k3d cluster create --config .\k3dcluster-single.yaml

kubectl apply -f .\services\namespace.yaml -f .\services\database\ -f .\services\api\ -f .\services\image-store\ -f .\services\website\

kubectl apply -f .\services\setup\
```




## Apply all replciated services

```
k3d cluster create --config .\k3dcluster.yaml --volume $PWD/cluster-configs/loadbalancer.yaml:/var/lib/rancher/k3s/server/manifests/traefik-config.yaml@server:*

kubectl apply -f .\services\namespace.yaml -f .\services-replicated\database\ -f .\services-replicated\api\ -f .\services-replicated\image-store\ -f .\services-replicated\website\ 

kubectl apply -f .\services-replicated\setup\
```

```
k3d cluster delete street-view && k3d cluster create --config .\k3dcluster.yaml --volume $PWD/cluster-configs/loadbalancer.yaml:/var/lib/rancher/k3s/server/manifests/traefik-config.yaml@server:* && kubectl apply -f .\services\namespace.yaml -f .\services-replicated\database\ -f .\services-replicated\api\ -f .\services-replicated\image-store\ -f .\services-replicated\website\
```



Run multi cluster tests:
```
./run_all.sh http://192.168.0.2:8080 >> multi-terminal.log 2>&1
```

Run single cluster tests:
```
./run_all.sh http://192.168.0.2:8090 >> single-terminal.log 2>&1
```

kubectl run tmp-shell --rm -i --tty --image nicolaka/netshoot

https://github.com/prometheus/node_exporter
https://github.com/prometheus-community/windows_exporter



winget install Prometheus.WindowsExporter

```
docker run -d --net="host" --pid="host" -v "/:/host:ro,rslave" quay.io/prometheus/node-exporter:latest --path.rootfs=/host
```

## Metrics

```
# Windows node
sum by (mode) (rate(windows_cpu_time_total{instance=~"host.k3d.internal:9182"}[5m]))

# cpu util
100 - avg(irate(windows_cpu_time_total{instance=~"192.168.0.173:9182",mode="idle"}[5m]))*100

# network sent bytes
irate(windows_net_bytes_sent_total{job=~"prometheus",instance=~"192.168.0.173:9182",nic!~'isatap.*|VPN.*'}[5m])*8>0

# network recived bytes
irate(windows_net_bytes_received_total{job=~"prometheus",instance=~"192.168.0.173:9182",nic!~'isatap.*|VPN.*'}[5m])*8
```