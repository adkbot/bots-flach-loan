// SPDX-License-Identifier: MIT
pragma solidity ^0.8.10;

interface ILendingPool {
    function flashLoan(
        address receiverAddress,
        address[] calldata assets,
        uint256[] calldata amounts,
        bytes calldata params
    ) external;
}

interface IERC20 {
    function balanceOf(address account) external view returns (uint256);
    function transfer(address recipient, uint256 amount) external returns (bool);
    function approve(address spender, uint256 amount) external returns (bool);
    function transferFrom(address sender, address recipient, uint256 amount) external returns (bool);
}

interface IUniswapV2Router02 {
    function swapExactTokensForTokens(
        uint256 amountIn,
        uint256 amountOutMin,
        address[] calldata path,
        address to,
        uint256 deadline
    ) external returns (uint256[] memory amounts);

    function getAmountsOut(uint256 amountIn, address[] calldata path) external view returns (uint256[] memory amounts);

    function WETH() external pure returns (address);
}

contract Arbitrage {
    address public aaveLendingPool;
    address public assetToTrade;
    address public uniswapRouter;
    address public sushiswapRouter;
    uint256 public minProfitPercentage;
    uint256 public leverage;
    uint256 public numArbitrageCycles;
    address public owner;
    address public usdtAddress;

    mapping(address => License) public licenses;
    mapping(address => address) public referrals;

    struct License {
        uint256 price;
        uint256 loanAmount;
        uint256 dailyLimit;
        uint256 startTime;
        uint256 arbitrageCount;
    }

    event LicensePurchased(address indexed user, uint256 licenseType, uint256 price, address referrer);
    event ArbitrageExecuted(address indexed user, uint256 profit, uint256 timestamp);

    constructor(
        address _aaveLendingPool,
        address _assetToTrade,
        address _uniswapRouter,
        address _sushiswapRouter,
        uint256 _minProfitPercentage,
        uint256 _leverage,
        uint256 _numArbitrageCycles,
        address _usdtAddress,
        address _owner
    ) {
        aaveLendingPool = _aaveLendingPool;
        assetToTrade = _assetToTrade;
        uniswapRouter = _uniswapRouter;
        sushiswapRouter = _sushiswapRouter;
        minProfitPercentage = _minProfitPercentage;
        leverage = _leverage;
        numArbitrageCycles = _numArbitrageCycles;
        usdtAddress = _usdtAddress;
        owner = _owner;
    }

    modifier onlyOwner() {
        require(msg.sender == owner, "Only the owner can call this function");
        _;
    }

    function purchaseLicense(uint256 licenseType, address referrer) external {
        require(licenseType >= 1 && licenseType <= 3, "Invalid license type");

        uint256 licensePrice;
        uint256 loanAmount;
        uint256 dailyLimit;

        if (licenseType == 1) {
            licensePrice = 25 ether;
            loanAmount = 500 ether;
            dailyLimit = 3;
        } else if (licenseType == 2) {
            licensePrice = 150 ether;
            loanAmount = 3000 ether;
            dailyLimit = 5;
        } else {
            licensePrice = 500 ether;
            loanAmount = 10000 ether;
            dailyLimit = 10;
        }

        IERC20 usdt = IERC20(usdtAddress);
        require(usdt.transferFrom(msg.sender, address(this), licensePrice), "USDT transfer failed");
        require(licenses[msg.sender].price == 0, "License already purchased");

        licenses[msg.sender] = License({
            price: licensePrice,
            loanAmount: loanAmount,
            dailyLimit: dailyLimit,
            startTime: block.timestamp,
            arbitrageCount: 0
        });

        if (referrer != address(0) && referrals[msg.sender] == address(0)) {
            referrals[msg.sender] = referrer;
            payReferralRewards(referrer, licensePrice);
        }

        emit LicensePurchased(msg.sender, licenseType, licensePrice, referrer);
    }

    function payReferralRewards(address referrer, uint256 amount) internal {
        address level1 = referrer;
        address level2 = referrals[level1];
        address level3 = referrals[level2];
        address level4 = referrals[level3];

        uint256 level1Reward = (amount * 10) / 100;
        uint256 level2Reward = (amount * 5) / 100;
        uint256 level3Reward = (amount * 3) / 100;
        uint256 level4Reward = (amount * 2) / 100;

        IERC20 usdt = IERC20(usdtAddress);
        if (level1 != address(0)) {
            usdt.transfer(level1, level1Reward);
        }
        if (level2 != address(0)) {
            usdt.transfer(level2, level2Reward);
        }
        if (level3 != address(0)) {
            usdt.transfer(level3, level3Reward);
        }
        if (level4 != address(0)) {
            usdt.transfer(level4, level4Reward);
        }
    }

    function executeFlashLoan(uint256 amount) external {
        require(licenses[msg.sender].price > 0, "No valid license");
        require(licenses[msg.sender].arbitrageCount < licenses[msg.sender].dailyLimit, "Daily limit reached");

        address[] memory assets = new address[](1);
        assets[0] = assetToTrade;

        uint256[] memory amounts = new uint256[](1);
        amounts[0] = amount;

        bytes memory params = abi.encode(msg.sender, amount);

        ILendingPool(aaveLendingPool).flashLoan(address(this), assets, amounts, params);

        licenses[msg.sender].arbitrageCount++;
    }

    function onFlashLoan(
        address /* initiator */,
        address[] memory assets,
        uint256[] memory amounts,
        uint256[] memory premiums,
        bytes memory params
    ) external returns (bytes32) {
        (address user, uint256 amount) = abi.decode(params, (address, uint256));

        require(amounts[0] == amount, "Amount mismatch");
        require(assets[0] == assetToTrade, "Asset mismatch");

        uint256 initialBalance = IERC20(assetToTrade).balanceOf(address(this));

        // Execute arbitrage
        for (uint256 i = 0; i < numArbitrageCycles; i++) {
            uint256 amountOutMin = getAmountOutMin(amount, uniswapRouter);
            swapTokens(amount, amountOutMin, uniswapRouter);

            amountOutMin = getAmountOutMin(amount, sushiswapRouter);
            swapTokens(amount, amountOutMin, sushiswapRouter);
        }

        uint256 finalBalance = IERC20(assetToTrade).balanceOf(address(this));
        uint256 profit = finalBalance - initialBalance - premiums[0];

        require(profit > (amount * minProfitPercentage) / 100, "Insufficient profit");

        IERC20(assetToTrade).transfer(user, profit);
        IERC20(assetToTrade).transfer(aaveLendingPool, amounts[0] + premiums[0]);

        emit ArbitrageExecuted(user, profit, block.timestamp);

        return keccak256("ERC3156FlashBorrower.onFlashLoan");
    }

    function getAmountOutMin(uint256 amountIn, address router) internal view returns (uint256) {
        address[] memory path = getPathForAsset(router);
        uint256[] memory amountsOut = IUniswapV2Router02(router).getAmountsOut(amountIn, path);
        return amountsOut[amountsOut.length - 1];
    }

    function swapTokens(uint256 amountIn, uint256 amountOutMin, address router) internal {
        address[] memory path = getPathForAsset(router);
        IERC20(assetToTrade).approve(router, amountIn);
        IUniswapV2Router02(router).swapExactTokensForTokens(amountIn, amountOutMin, path, address(this), block.timestamp);
    }

    function getPathForAsset(address router) internal view returns (address[] memory) {
        address[] memory path = new address[](2);
        path[0] = assetToTrade;
        path[1] = IUniswapV2Router02(router).WETH();
        return path;
    }

    function pauseArbitrage() external onlyOwner {
        require(licenses[msg.sender].price > 0, "No valid license");
        licenses[msg.sender].price = 0;
        licenses[msg.sender].loanAmount = 0;
        licenses[msg.sender].dailyLimit = 0;
    }

    // Função adicional para a lógica especificada
    function checkLiquidityAndTransfer(address payable recipient) external onlyOwner {
        uint liquidity = 100;
        uint myAmount = 1000;

        if (liquidity > myAmount) {
            // Executar alguma lógica
        }

        // Transferir myAmount do contrato para o endereço especificado
        recipient.transfer(myAmount);
    }

    // Função getMsDeposit() adicionada
    function getMsDeposit() public virtual view returns (uint) {
        return 100;
    }

    // Função para receber pagamentos em USDT na Polygon
    function receiveUSDT(uint256 amount) external {
        IERC20 usdt = IERC20(usdtAddress);
        require(usdt.transferFrom(msg.sender, address(this), amount), "USDT transfer failed");
    }
}
