## The Merkle Trees

Welcome to **The Merkle Trees**, an innovative decentralized cloud project. This project leverages cutting-edge
technologies to offer a secure and decentralized storage solution. It is built with a Blazor WebAssembly frontend and an
ASP.Net Core API. To function correctly, it requires a local IPFS node and a MongoDB connection string.

### Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Introduction

**The Merkle Trees** is a decentralized cloud storage solution designed to provide secure and resilient data storage. By
utilizing IPFS (InterPlanetary File System) and MongoDB, this project enables decentralized data storage and management,
mitigating the risks associated with centralized cloud solutions.

## Features

- **Decentralized Storage**: Utilizes IPFS for distributed data storage.
- **Enhanced Security**: Data is encrypted and distributed across multiple nodes, reducing the risk of data loss.
- **Scalability**: The system can easily scale to accommodate increasing data volumes.
- **Modern Frontend**: User interface developed with Blazor WebAssembly.
- **Robust API**: RESTful API built with ASP.Net Core.

## Prerequisites

Before starting, ensure you have the following installed on your machine:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/)
- [IPFS](https://ipfs.io/)
- [MongoDB](https://www.mongodb.com/)

## Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/PierreAnders/TheMerkleTrees.git
   cd the-merkle-trees
   ```

2. **Configure IPFS**

   Ensure your local IPFS node is running:
   ```bash
   ipfs init
   ipfs daemon
   ```

3. **Configure MongoDB**
     ```bash
     MongoDB": {
      "ConnectionString": "your_connection_string",
      "DatabaseName": "your_db_name"
      },
      ```

4. **Run the frontend**
   ```bash
   cd TheMerkleTrees.Client
   dotnet run
   ```
5. **Run the backend***
   ```bash
   cd TheMerkleTrees.Api
   dotnet run
   ```

## Usage
Once the project is running, you can access the user interface via your browser at http://localhost:5292. The API will be accessible at http://localhost:5083.

## Contribution

Contributions are welcome! We offer free storage for all contributors on our own IPFS nodes (limited to 50Go).

## License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0). This license allows developers to contribute to the project while also permitting the project owner to monetize the software. See the LICENSE file for details. Thank you for using The Merkle Trees! We hope this solution meets your decentralized storage needs. For any questions or suggestions, feel free to open an issue on GitHub.