# Kubernetes cluster

## Prerequisite

- [Virtual Box](https://www.virtualbox.org/) 
  - exec by [administrator]
- [Vagrant](https://www.vagrantup.com/downloads)
  - [[win] install issues](https://www.youtube.com/watch?v=oqmVOLY3g3k)
- [[SSH tools] MobaXterm](https://mobaxterm.mobatek.net/download.html)

## Vagrant VM templates

### [kodekloudhub/certified-kubernetes-administrator-course](https://github.com/kodekloudhub/certified-kubernetes-administrator-course)

- Clone project
```
git clone https://github.com/kodekloudhub/certified-kubernetes-administrator-course.git
```

- Move path /certified-kubernetes-administrator-course
```
# VM status
vagrant status

# VM setup
vagrant up

# VM ssh
vagrant ssh [VM-name]

# VM shutdown
vagrant halt

# VM delete
vagrant destroy [VM-name]
```

## Env setup 

### VM setup

- kubemaster
- kubenode01
- kubenode02

### [Installing kubeadm](https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/install-kubeadm/)

- Letting iptables see bridged traffic
```
sudo modprobe br_netfilter

lsmod | grep br_netfilter
```

```
cat <<EOF | sudo tee /etc/modules-load.d/k8s.conf
br_netfilter
EOF

cat <<EOF | sudo tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables = 1
EOF
sudo sysctl --system
```
---
## Docker setup

### VM setup

- kubemaster
- kubenode01
- kubenode02

### [install docker-ce at ubuntu](https://docs.docker.com/engine/install/ubuntu/)

- Set up the repository
```
sudo apt-get update
sudo apt-get install \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release
```

- Add Docker’s official GPG key
```
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
```

- Use the following command to set up the stable repository
```
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```

- Install Docker Engine
```
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io
```

- To install a specific version of Docker Engine, list the available versions in the repo
```
apt-cache madison docker-ce
```

- Install a specific version using the version string
```
sudo apt-get install docker-ce=5:19.03.15~3-0~ubuntu-bionic docker-ce-cli=5:19.03.15~3-0~ubuntu-bionic containerd.io
```

### [install container runtime](https://kubernetes.io/docs/setup/production-environment/container-runtimes/#docker)

- Configure the Docker daemon
```
sudo mkdir /etc/docker
cat <<EOF | sudo tee /etc/docker/daemon.json
{
  "exec-opts": ["native.cgroupdriver=systemd"],
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m"
  },
  "storage-driver": "overlay2"
}
EOF
```

- Restart Docker and enable on boot
```
sudo systemctl enable docker
sudo systemctl daemon-reload
sudo systemctl restart docker
```

### [Installing kubeadm, kubelet and kubectl](https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/install-kubeadm/#installing-kubeadm-kubelet-and-kubectl)

- Update the apt package index and install packages needed to use the Kubernetes apt repository
```
sudo apt-get update
sudo apt-get install -y apt-transport-https ca-certificates curl
```

- Download the Google Cloud public signing key
```
sudo curl -fsSLo /usr/share/keyrings/kubernetes-archive-keyring.gpg https://packages.cloud.google.com/apt/doc/apt-key.gpg
```

- Add the Kubernetes apt repository
```
echo "deb [signed-by=/usr/share/keyrings/kubernetes-archive-keyring.gpg] https://apt.kubernetes.io/ kubernetes-xenial main" | sudo tee /etc/apt/sources.list.d/kubernetes.list
```

- Update apt package index, install kubelet, kubeadm and kubectl, and pin their version
```
sudo apt-get update
sudo apt-get install -y kubelet kubeadm kubectl
sudo apt-mark hold kubelet kubeadm kubectl
```
---




## Creating a cluster with kubeadm

### VM setup

- kubemaster

### Initializing your control-plane node

- display apiserver host
```
ifconfig enp0s8 # inet 192.168.56.2
```

- Initialize the master
```
kubeadm init --apiserver-advertise-address=192.168.56.2 --pod-network-cidr=10.244.0.0/16
```

- Make kubectl work with context config
```
mkdir -p $HOME/.kube
sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config
```

- Network setup [Weave](https://www.weave.works/docs/net/latest/kubernetes/kube-addon/)
```
kubectl apply -f "https://cloud.weave.works/k8s/net?k8s-version=$(kubectl version | base64 | tr -d '\n')"
```
---
## WorkerNode joined

### VM setup

- kubenode01
- kubenode02
  
### [Token joined](https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/#join-nodes)
```
kubeadm join 192.168.56.2:6443 --token buxhhg.d5j7uw8lzj7cderj \
        --discovery-token-ca-cert-hash sha256:61c317cf782ca6d115306efb781cc6798c545b3fc6d2345640f2acf36d0a7a06
```
