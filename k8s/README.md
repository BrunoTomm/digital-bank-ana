# BankMore - Manifests Kubernetes (Produção)

Manifests para deploy do BankMore em um cluster Kubernetes (produção ou staging).

## Pré-requisitos

- `kubectl` configurado
- Namespace `bankmore` será criado pelos manifests
- Imagens Docker das APIs publicadas em um registry acessível ao cluster
- (Opcional) Ingress Controller (nginx) para expor as APIs externamente

## Ordem de aplicação

Aplique os arquivos nesta ordem:

```bash
# 1. Namespace
kubectl apply -f namespace.yaml

# 2. Secrets e ConfigMaps
kubectl apply -f secret.yaml
kubectl apply -f configmap.yaml

# 3. Infraestrutura - Oracle
kubectl apply -f oracle-pvc.yaml
kubectl apply -f oracle-deployment.yaml
kubectl apply -f oracle-service.yaml

# 4. Infraestrutura - Kafka
kubectl apply -f kafka-pvc.yaml
kubectl apply -f kafka-deployment.yaml
kubectl apply -f kafka-service.yaml

# 5. Jobs de inicialização (aguardar Oracle e Kafka subirem)
kubectl apply -f oracle-init-job.yaml
kubectl apply -f kafka-init-job.yaml

# Aguardar os jobs concluírem
kubectl wait --for=condition=complete job/oracle-init -n bankmore --timeout=300s
kubectl wait --for=condition=complete job/kafka-init -n bankmore --timeout=120s

# 6. APIs
kubectl apply -f current-account-deployment.yaml
kubectl apply -f transfer-deployment.yaml
kubectl apply -f fees-deployment.yaml

# 7. (Opcional) Ingress
kubectl apply -f ingress.yaml
```

## Imagens das APIs

Antes de aplicar os Deployments, faça build e push das imagens:

```bash
# Exemplo com registry local ou seu registry
REGISTRY=seu-registry.io/bankmore

docker build -t $REGISTRY/current-account:latest ./bank-more-current-account
docker build -t $REGISTRY/transfer:latest ./bank-more-transfer
docker build -t $REGISTRY/fees:latest ./bank-more-fees

docker push $REGISTRY/current-account:latest
docker push $REGISTRY/transfer:latest
docker push $REGISTRY/fees:latest
```

Em seguida, edite os manifests de Deployment para usar suas imagens:

```yaml
# Exemplo: current-account-deployment.yaml
containers:
  - name: current-account
    image: seu-registry.io/bankmore/current-account:latest
```

Ou use `kubectl set image` após o apply:

```bash
kubectl set image deployment/bank-more-current-account current-account=seu-registry.io/bankmore/current-account:latest -n bankmore
kubectl set image deployment/bank-more-transfer transfer=seu-registry.io/bankmore/transfer:latest -n bankmore
kubectl set image deployment/bank-more-fees fees=seu-registry.io/bankmore/fees:latest -n bankmore
```

## Ingress e acesso externo

O `ingress.yaml` expõe as APIs com subdomínios:

- **Current Account**: `http://current-account.bankmore.local`
- **Transfer**: `http://transfer.bankmore.local`
- **Fees**: `http://fees.bankmore.local`

Configure `/etc/hosts` ou DNS:

```
127.0.0.1 current-account.bankmore.local transfer.bankmore.local fees.bankmore.local
```

Se usar outro Ingress Controller, ajuste `kubernetes.io/ingress.class` e o host conforme necessário.

## Secrets

O `secret.yaml` contém valores sensíveis. Em produção:

1. Use um Secret Manager (ex.: External Secrets Operator, Sealed Secrets)
2. Ou gere o secret via `kubectl create secret` e remova `secret.yaml` do repositório
3. A chave `oracle-connection` deve ter a connection string completa:  
   `User Id=bankmore;Password=<senha>;Data Source=oracle:1521/XEPDB1`

## Verificação

```bash
kubectl get all -n bankmore
kubectl get ingress -n bankmore
kubectl logs -l app=bank-more-current-account -n bankmore -f
```
