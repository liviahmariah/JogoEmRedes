using Unity.Netcode;
using UnityEngine;

public class LebreController : NetworkBehaviour
{
    public float velocidadeCorrida = 5f;
    public float distanciaFaixa = 1.5f; // Distância ao subir/descer
    private int faixaAtual = 0; // -1 (baixo), 0 (meio), 1 (cima)

    public override void OnNetworkSpawn()
    {
        // Garante que a câmera siga apenas a lebre do jogador local
        if (IsOwner)
        {
            Camera.main.transform.SetParent(transform);
            // Ajuste o X (ex: 3f) para a lebre ficar um pouco à esquerda da tela, mostrando os obstáculos à frente
            Camera.main.transform.localPosition = new Vector3(3f, 0f, -10f);
        }
    }

    void Update()
    {
        // Só executa o código se este objeto pertencer ao jogador local
        if (!IsOwner) return;

        // Avanço automático constante para a direita (corrida)
        transform.Translate(Vector2.right * velocidadeCorrida * Time.deltaTime);

        // Controles definidos no GDD: apenas setas cima e baixo
        if (Input.GetKeyDown(KeyCode.UpArrow) && faixaAtual < 1)
        {
            faixaAtual++;
            AtualizarPosicaoY();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && faixaAtual > -1)
        {
            faixaAtual--;
            AtualizarPosicaoY();
        }
    }

    void AtualizarPosicaoY()
    {
        Vector3 novaPos = transform.position;
        novaPos.y = faixaAtual * distanciaFaixa;
        transform.position = novaPos;
    }

    // Colisão com obstáculos ou linha de chegada
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Obstaculo"))
        {
            // Reduz velocidade temporariamente ao bater
            velocidadeCorrida = 2f;
            Invoke(nameof(RestaurarVelocidade), 1.5f);
        }
        else if (other.CompareTag("LinhaChegada"))
        {
            // Avisa o servidor que este jogador venceu
            DeclararVencedorServerRpc(OwnerClientId);
        }
    }

    void RestaurarVelocidade() => velocidadeCorrida = 5f;

    [ServerRpc]
    void DeclararVencedorServerRpc(ulong clientId)
    {
        // O servidor avisa a todos os clientes quem ganhou
        AnunciarVencedorClientRpc(clientId);
    }

    [ClientRpc]
    void AnunciarVencedorClientRpc(ulong vencedorId)
    {
        Debug.Log($"O Jogador {vencedorId} venceu a corrida!");
        // Aqui você ativa a tela de Vitória/Derrota na UI
    }
}