# News & Reddit Trend Analyzer – ETL com .NET, KNIME e MongoDB

## 1. Identificação do Autor

- **Nome:** Diogo Carvalho  
- **Curso:** Licenciatura em Engenharia de Sistemas Informáticos  
- **Instituição:** Instituto Politécnico do Cávado e do Ave (IPCA)  
- **Unidade Curricular:** Integração de Sistemas de Informação – 2025/2026

## 2. Descrição Geral do Projeto

Este projeto consiste na implementação de um sistema de ETL (Extract, Transform, Load) que permite recolher, processar e analisar dados de notícias e publicações online, com o objetivo de medir a **popularidade** e o **sentimento público** sobre um determinado tema.

### **Componentes principais:**
| Componente      | Descrição |
|-----------------|-----------|
| **API .NET**    | Agrega dados da NewsAPI e da Reddit API e devolve um JSON unificado. |
| **KNIME Workflow** | Processa os dados: limpeza, conversão de datas, cálculo de sentimento (Python), popularidade por dia e inserção em MongoDB. |
| **MongoDB**     | Armazena os resultados do processamento (`topic_trends`) e logs de execução (`execution_logs`). |
| **Job C# (KnimeRunner)** | Automatiza a execução diária do workflow KNIME. |
| **Visualização (opcional)** | Possível integração futura com Power BI ou Grafana para dashboards. |

---

## 3. Estrutura dos Ficheiros
 Projeto
├── README.md
├── /src/ API_NewsAggregator → Projeto .NET (Controllers, Services, Models)
├── /src/ Job_KnimeRunner → Código C# para execução automática
├── /dataint/ KNIME_ISI_Projeto → Workflow KNIME (.knwf)
├── data/output/ topic_trends.json (colections Mongo DB)
├── data/output/ execution_logs
└── /doc/ → Relatório

##  4. Tecnologias Utilizadas

| Tecnologia        | Função |
|-------------------|--------|
| **.NET 8 / C#**   | API de agregação e automação|
| **Visual Studio 2022** | IDE para desenvolver API e job KnimeRunner. |
| **KNIME Analytics Platform** | Ferramenta de ETL para transformação de dados. |
| **Python (via KNIME)** | Cálculo de sentimento com TextBlob + GoogleTranslator. |
| **MongoDB + Compass** | Base de dados não relacional para armazenamento dos resultados. |
| **NewsAPI & Reddit API** | Fontes externas de dados (notícias + redes sociais). |

## 5. Como Executar a Solução

### **Opção 1 – Execução Manual**

1. **Colocar a API .NET a correr**  
   - Abrir a solução no Visual Studio  
   - Definir projeto como Startup  
   - Executar (F5 ou Ctrl+F5)  
   - Endpoint padrão: `https://localhost:xxxx/api/news`
   - Exemplo de enpoint a executar:`https://localhost:xxxx/api/News?topic=crypto&days=7&pageSize=100&pageNumber=1&language=pt&sortBy=popularity` 

2. **Executar o workflow no KNIME**  
   - Abrir `KNIME_ISI_Projeto.knwf` no KNIME  
   - Definir o topic no endpoint dentro do nó GET request  
   - Clicar em Execute All

3. **MongoDB**  
   - Ter o serviço **MongoDB** a correr localmente ou numa cloud  
   - Garantir a conexão a DB
   - As coleções serão automaticamente criadas:  
     - `topic_trends`  
     - `execution_logs`

---

### **Opção 2 – Execução Automática com Job**

1. Abrir o projeto `KnimeRunner` no Visual Studio 2022  
2. Alterar caminhos de instalação se necessário:  
   ```csharp
   private static string KNIME_PATH = @"C:\...\knime.exe";
   private static string WORKFLOW_PATH = @"C:\...\KNIME_ISI_Projeto";
   private static string LOG_PATH = @"C:\...\log.txt";


### Link para o video de demonstração

- https://youtu.be/NIS6dQz5FXQ
