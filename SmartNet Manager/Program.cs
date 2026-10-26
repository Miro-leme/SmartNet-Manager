using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartNetManager
{
    // O Modelo de Domínio (A Classe Base)
    public class Dispositivo
    {
        public string MacAddress { get; set; } // Ex: "AA:BB:CC:11:22"
        public string EnderecoIP { get; set; } // Ex: "192.168.0.10"
        public string Nome { get; set; }       // Ex: "PC-Contabilidade"
        public string Fabricante { get; set; } // Ex: "Dell", "Apple", "Samsung"
        public double ConsumoBanda { get; set; } // Em Mbps
        public DateTime DataConexao { get; set; }
    }

    class Program
    {
        // Estruturas de Dados do Sistema
        private static Dictionary<string, string> _servidorDHCP = new Dictionary<string, string>(); // Missão 1
        private static Queue<Dispositivo> _filaAutenticacao = new Queue<Dispositivo>();             // Missão 2
        private static List<Dispositivo> _dispositivosAtivos = new List<Dispositivo>();             // Missão 3
        private static Stack<string> _historicoAuditoria = new Stack<string>();                     // Missão 4

        static void Main(string[] args)
        {
            // Inicialização com dados fictícios
            CarregarDadosIniciais();

            bool executando = true;
            while (executando)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==================================================");
                Console.WriteLine("                 SMARTNET MANAGER                 ");
                Console.WriteLine("==================================================");
                Console.ResetColor();
                Console.WriteLine("1 - Cadastrar Reserva de IP (DHCP)");
                Console.WriteLine("2 - Adicionar Dispositivo à Fila de Autenticação");
                Console.WriteLine("3 - Processar Próximo da Fila (Autenticar QoS)");
                Console.WriteLine("4 - Exibir Dashboard Estatístico");
                Console.WriteLine("5 - Gerar Relatórios Universais (Fila / Ativos)");
                Console.WriteLine("6 - Aplicar Regras de Firewall Dinâmico");
                Console.WriteLine("7 - Desfazer Última Ação (Rollback)");
                Console.WriteLine("0 - Sair");
                Console.WriteLine("--------------------------------------------------");
                Console.Write("Escolha uma opção: ");

                string opcao = Console.ReadLine();
                Console.WriteLine();

                switch (opcao)
                {
                    case "1": CadastrarReservaDHCP(); break;
                    case "2": AdicionarAFiladeAutenticacao(); break;
                    case "3": ProcessarFilaAutenticacao(); break;
                    case "4": ExibirDashboard(); break;
                    case "5": MenuRelatorios(); break;
                    case "6": MenuFirewall(); break;
                    case "7": DesfazerUltimaAcao(); break;
                    case "0": executando = false; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Opção inválida! Pressione qualquer tecla para tentar novamente.");
                        Console.ResetColor();
                        Console.ReadKey();
                        break;
                }
            }
        }

        //Missão 1: Servidor DHCP (Reserva de IP)
        private static void CadastrarReservaDHCP()
        {
            Console.WriteLine("--- CADASTRO DE RESERVA DHCP ---");
            Console.Write("Digite o MAC Address (Ex: AA:BB:CC:11:22): ");
            string mac = Console.ReadLine().Trim().ToUpper();
            Console.Write("Digite o Endereço IP (Ex: 192.168.0.10): ");
            string ip = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(mac) || string.IsNullOrEmpty(ip))
            {
                ExibirMensagemErro("MAC ou IP não podem ser vazios.");
                return;
            }

            // Regra: O sistema não pode permitir que o mesmo MAC seja cadastrado duas vezes.
            if (_servidorDHCP.ContainsKey(mac))
            {
                ExibirMensagemErro($"Erro: O MAC Address {mac} já possui uma reserva de IP definida ({_servidorDHCP[mac]}).");
            }
            else
            {
                _servidorDHCP.Add(mac, ip);
                ExibirMensagemSucesso($"Sucesso: Reserva criada! MAC {mac} -> IP {ip}");
            }
        }
        

        //Missão 2: Fila de Autenticação (QoS)
        private static void AdicionarAFiladeAutenticacao()
        {
            Console.WriteLine("--- ADICIONAR DISPOSITIVO À FILA ---");
            Console.Write("Nome do Dispositivo: ");
            string nome = Console.ReadLine();
            Console.Write("MAC Address: ");
            string mac = Console.ReadLine().Trim().ToUpper();
            Console.Write("Fabricante (Dell, Apple, Samsung, etc.): ");
            string fabricante = Console.ReadLine();
            Console.Write("Consumo de Banda Atual (Mbps): ");
            double.TryParse(Console.ReadLine(), out double banda);

            // Verifica se o MAC possui IP reservado no DHCP, caso contrário gera um IP dinâmico
            string ipAtribuido = _servidorDHCP.ContainsKey(mac) ? _servidorDHCP[mac] : "192.168.1." + new Random().Next(100, 254);

            Dispositivo novo = new Dispositivo
            {
                MacAddress = mac,
                EnderecoIP = ipAtribuido,
                Nome = nome,
                Fabricante = fabricante,
                ConsumoBanda = banda,
                DataConexao = DateTime.Now
            };

            _filaAutenticacao.Enqueue(novo);
            ExibirMensagemSucesso($"Dispositivo '{novo.Nome}' adicionado à Fila de Espera com o IP {novo.EnderecoIP}.");
        }

        private static void ProcessarFilaAutenticacao()
        {
            Console.WriteLine("--- PROCESSANDO FILA DE AUTENTICAÇÃO (QoS) ---");

            if (_filaAutenticacao.Count == 0)
            {
                ExibirMensagemErro("A fila de autenticação está vazia.");
                return;
            }

            // Regra: remove (Dequeue) o dispositivo mais antigo
            Dispositivo dispositivoAutenticado = _filaAutenticacao.Dequeue();

            // Envia para a Lista de Ativos
            _dispositivosAtivos.Add(dispositivoAutenticado);

            // Registro no Histórico de Auditoria (Missão 4)
            string mensagemLog = $"AUTENTICACAO:{dispositivoAutenticado.MacAddress}";
            _historicoAuditoria.Push(mensagemLog);

            ExibirMensagemSucesso($"Dispositivo '{dispositivoAutenticado.Nome}' ({dispositivoAutenticado.MacAddress}) autenticado com sucesso e enviado para a rede ativa!");
        }
        

        // Missão 3: Conexões Ativas e Dashboard
        private static void ExibirDashboard()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("             DASHBOARD DE REDE (LINQ)             ");
            Console.WriteLine("==================================================");

            if (_dispositivosAtivos.Count == 0)
            {
                Console.WriteLine("Nenhum dispositivo online no momento.");
                Console.ReadKey();
                return;
            }

            // 1. O consumo total de banda atual
            double consumoTotal = _dispositivosAtivos.Sum(d => d.ConsumoBanda);
            Console.WriteLine($"Consumo Total de Banda Atual: {consumoTotal:F2} Mbps");
            Console.WriteLine("--------------------------------------------------");

            // 2. O Top 3 dispositivos que mais estão consumindo internet
            Console.WriteLine("TOP 3 Consumidores de Banda:");
            var top3 = _dispositivosAtivos.OrderByDescending(d => d.ConsumoBanda).Take(3);
            int ranking = 1;
            foreach (var d in top3)
            {
                Console.WriteLine($"  {ranking}º - {d.Nome} ({d.Fabricante}) -> {d.ConsumoBanda} Mbps");
                ranking++;
            }
            Console.WriteLine("--------------------------------------------------");

            // 3. Quantos aparelhos de cada Fabricante estão conectados
            Console.WriteLine("Dispositivos por Fabricante:");
            var porFabricante = _dispositivosAtivos.GroupBy(d => d.Fabricante);
            foreach (var grupo in porFabricante)
            {
                Console.WriteLine($"  * {grupo.Key}: {grupo.Count()} aparelho(s)");
            }
            Console.WriteLine("--------------------------------------------------");

            // 4. Um alerta de anomalia se algum dispositivo estiver consumindo mais de 500 Mbps
            var anomalias = _dispositivosAtivos.Where(d => d.ConsumoBanda > 500).ToList();
            if (anomalias.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ ALERTA DE ANOMALIA DETECTADO!");
                foreach (var d in anomalias)
                {
                    Console.WriteLine($"  [!] ALERTA: {d.Nome} (MAC: {d.MacAddress}) está consumindo {d.ConsumoBanda} Mbps! (Limite: 500 Mbps)");
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Tráfego estável. Nenhuma anomalia de banda detectada.");
                Console.ResetColor();
            }

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }
        

        // Missão 4: Histórico de Auditoria (Rollback)
        private static void DesfazerUltimaAcao()
        {
            Console.WriteLine("--- DESFAZER ÚLTIMA AÇÃO (ROLLBACK) ---");

            if (_historicoAuditoria.Count == 0)
            {
                ExibirMensagemErro("Não há ações no histórico para desfazer.");
                return;
            }

            string ultimaAcao = _historicoAuditoria.Pop();
            string[] dados = ultimaAcao.Split(':');
            string tipoAcao = dados[0]; 

            if (tipoAcao == "AUTENTICACAO")
            {
                string mac = dados[1]; 
                                       
                Dispositivo disp = _dispositivosAtivos.FirstOrDefault<Dispositivo>(d => d.MacAddress == mac);
                if (disp != null)
                {
                    _dispositivosAtivos.Remove(disp);
                    _filaAutenticacao.Enqueue(disp);
                    ExibirMensagemSucesso($"Rollback Concluído: Autenticação do dispositivo {disp.Nome} ({mac}) foi desfeita. Ele retornou para a Fila de Espera.");
                }
                else
                {
                    ExibirMensagemErro("O dispositivo associado a esta ação não foi encontrado nos ativos.");
                }
            }
            else if (tipoAcao == "FIREWALL")
            {
                if (dados.Length > 1 && !string.IsNullOrEmpty(dados[1]))
                {
                    string[] dispositivosRecuperados = dados[1].Split('|');
                    foreach (var dispStr in dispositivosRecuperados)
                    {
                        if (string.IsNullOrEmpty(dispStr)) continue;
                        string[] campos = dispStr.Split(';');
                        _dispositivosAtivos.Add(new Dispositivo
                        {
                            Nome = campos[0],
                            MacAddress = campos[1],
                            EnderecoIP = campos[2],
                            Fabricante = campos[3],
                            ConsumoBanda = double.Parse(campos[4]),
                            DataConexao = DateTime.Now
                        });
                    }
                    ExibirMensagemSucesso($"Rollback Concluído: Regra de Firewall desfeita. {dispositivosRecuperados.Length} dispositivo(s) foram restaurados à rede.");
                }
                else
                {
                    ExibirMensagemSucesso("Rollback Concluído: A última regra de firewall não havia removido nenhum dispositivo.");
                }
            }
        }
        
        // Missão 5: O Gerador de Relatórios Universal
        private static void MenuRelatorios()
        {
            Console.WriteLine("--- GERADOR DE RELATÓRIOS UNIVERSAL ---");
            Console.WriteLine("1 - Imprimir Fila de Autenticação");
            Console.WriteLine("2 - Imprimir Lista de Dispositivos Ativos");
            Console.Write("Escolha qual relatório imprimir: ");
            string subOpcao = Console.ReadLine();
            Console.WriteLine();
            if (subOpcao == "1")
            {
                Console.WriteLine(">>> RELATÓRIO: FILA DE ESPERA (QoS) <<<");
                ImprimirRelatorio(_filaAutenticacao);
            }
            else if (subOpcao == "2")
            {
                Console.WriteLine(">>> RELATÓRIO: DISPOSITIVOS ONLINE <<<");
                ImprimirRelatorio(_dispositivosAtivos);
            }
            else
            {
                ExibirMensagemErro("Opção inválida.");
            }
        }
        // Exigência Técnica: Método recebendo IEnumerable utilizando um foreach
        public static void ImprimirRelatorio(IEnumerable<Dispositivo> colecao)
        {
            
            if (colecao == null || !colecao.Any<Dispositivo>())
            {
                Console.WriteLine("Nenhum dispositivo encontrado nesta coleção.");
                Console.WriteLine("\nPressione qualquer tecla para continuar.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(string.Format("{0,-18} | {1,-15} | {2,-15} | {3,-12} | {4,-10}", "Nome", "IP", "MAC", "Fabricante", "Banda"));
            Console.WriteLine(new string('-', 80));

            foreach (Dispositivo d in colecao)
            {
                Console.WriteLine(string.Format("{0,-18} | {1,-15} | {2,-15} | {3,-12} | {4,-10} Mbps",
                    d.Nome.Length > 18 ? d.Nome.Substring(0, 15) + "..." : d.Nome,
                    d.EnderecoIP,
                    d.MacAddress,
                    d.Fabricante,
                    d.ConsumoBanda));
            }

            Console.WriteLine("\nFim do relatório. Pressione qualquer tecla para voltar.");
            Console.ReadKey();
        }

        
        // Missão 6: Firewall Dinâmico
        private static void MenuFirewall()
        {
            Console.WriteLine("--- FIREWALL DINÂMICO ---");
            Console.WriteLine("1 - Bloquear aparelhos da fabricante 'Apple'");
            Console.WriteLine("2 - Bloquear aparelhos consumindo mais de 100 Mbps");
            Console.WriteLine("3 - Criar Regra Customizada por Nome de Dispositivo");
            Console.Write("Escolha a regra a aplicar: ");
            string escolha = Console.ReadLine();
            switch (escolha)
            {
                case "1":
                    // Expressão Lambda: Bloquear aparelhos da "Apple"
                    AplicarFirewall(d => d.Fabricante.Equals("Apple", StringComparison.OrdinalIgnoreCase));
                    break;
                case "2":
                    // Expressão Lambda: Bloquear aparelhos consumindo mais de 100 Mbps
                    AplicarFirewall(d => d.ConsumoBanda > 100);
                    break;
                case "3":
                    Console.Write("Digite o termo que o Nome do dispositivo deve conter para ser bloqueado: ");
                    string termo = Console.ReadLine();
                    AplicarFirewall(d => d.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
                    break;
                default:
                    ExibirMensagemErro("Regra inválida.");
                    break;
            }
        }
        // Exigência Técnica: Método AplicarFirewall recebendo Func<Dispositivo, bool>
        // Exigência Técnica: Garantido o uso explícito de Func<Dispositivo, bool>
        public static void AplicarFirewall(Func<Dispositivo, bool> regraBloqueio)
        {
            // Forçando o LINQ a saber que está tratando a lista com a extensão <Dispositivo>
            List<Dispositivo> paraRemover = _dispositivosAtivos.Where<Dispositivo>(regraBloqueio).ToList<Dispositivo>();

            if (paraRemover.Count == 0)
            {
                _historicoAuditoria.Push("FIREWALL:");
                ExibirMensagemSucesso("Filtro executado: Nenhum dispositivo ativo correspondeu à regra de bloqueio.");
                return;
            }

            // Serializa os dados básicos para permitir o Rollback (Missão 4)
            string dadosDispositivos = string.Join("|", paraRemover.Select(d => $"{d.Nome};{d.MacAddress};{d.EnderecoIP};{d.Fabricante};{d.ConsumoBanda}"));
            _historicoAuditoria.Push($"FIREWALL:{dadosDispositivos}");

            // Regra: Remove os dispositivos que caíram na regra
            foreach (Dispositivo d in paraRemover)
            {
                _dispositivosAtivos.Remove(d);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[BLOQUEADO] {d.Nome} ({d.EnderecoIP}) desconectado pelo Firewall.");
            }

            Console.ResetColor();
            ExibirMensagemSucesso($"\nFirewall aplicado! Total de {paraRemover.Count} dispositivo(s) banido(s) da rede.");
        }
        
        // Utilitários de Interface e Mock
        private static void ExibirMensagemSucesso(string mensagem)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{mensagem}");
            Console.ResetColor();
            Console.WriteLine("\nPressione qualquer tecla para continuar.");
            Console.ReadKey();
        }
        private static void ExibirMensagemErro(string mensagem)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{mensagem}");
            Console.ResetColor();
            Console.WriteLine("\nPressione qualquer tecla para continuar.");
            Console.ReadKey();
        }
        private static void CarregarDadosIniciais()
        {
            // Cadastra algumas reservas DHCP padrão
            _servidorDHCP.Add("00:1A:2B:3C:4D:5E", "192.168.0.100");
            _servidorDHCP.Add("AA:BB:CC:11:22:33", "192.168.0.150");
            _servidorDHCP.Add("11:22:33:44:55:66", "192.168.0.200");
            // Popula alguns dispositivos já Ativos
            _dispositivosAtivos.Add(new Dispositivo { Nome = "PC-Contabilidade", MacAddress = "00:1A:2B:3C:4D:5E", EnderecoIP = "192.168.0.100", Fabricante = "Dell", ConsumoBanda = 45.5, DataConexao = DateTime.Now });
            _dispositivosAtivos.Add(new Dispositivo { Nome = "iPhone-Diretoria", MacAddress = "AA:BB:CC:11:22:33", EnderecoIP = "192.168.0.150", Fabricante = "Apple", ConsumoBanda = 120.0, DataConexao = DateTime.Now });
            _dispositivosAtivos.Add(new Dispositivo { Nome = "SmartTV-Recepcao", MacAddress = "77:88:99:AA:BB:CC", EnderecoIP = "192.168.0.102", Fabricante = "Samsung", ConsumoBanda = 15.2, DataConexao = DateTime.Now });
            // Forçando um dispositivo com anomalia (> 500 Mbps) para testar o alerta do Dashboard
            _dispositivosAtivos.Add(new Dispositivo { Nome = "Servidor-Backup-Suspeito", MacAddress = "FF:FF:FF:FF:FF:FF", EnderecoIP = "192.168.0.250", Fabricante = "Dell", ConsumoBanda = 620.0, DataConexao = DateTime.Now });
            // Popula um dispositivo na fila de autenticação inicial
            _filaAutenticacao.Enqueue(new Dispositivo { Nome = "Notebook-Estagiario", MacAddress = "11:22:33:44:55:66", EnderecoIP = "192.168.0.200", Fabricante = "Lenovo", ConsumoBanda = 8.0, DataConexao = DateTime.Now });
        }
        
    }
}