#!/usr/bin/env bash

# API server and port
URL="https://localhost"
PORT="5001"


### DEFINING VARIABLES FOR ENDPOINTS

## Endpoint #1
# These variables are associsted with the /api/rebuild endpoint
# The URL where zip file will be downloaded from.
presignedURL1="https://glasswalltask1.s3.eu-west-1.amazonaws.com/file.pdf?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAWYWOGVNRJV2VKF6Y%2F20210122%2Feu-west-1%2Fs3%2Faws4_request&X-Amz-Date=20210122T230144Z&X-Amz-Expires=604800&X-Amz-SignedHeaders=host&X-Amz-Signature=2821576c7aea054fc38d2d6b90cd3af59da6f8bb8c5f0627d1a92b265b9defe3"
# The URL which the protected file will be uploaded to
targetPresignedURL1="https://glasswalltask1.s3.amazonaws.com/test_public?AWSAccessKeyId=AKIAWYWOGVNRJV2VKF6Y&Signature=sYPPidBgzr7XbZqfa6VUN0xNEH0%3D&Expires=1611961585"


## Endpoint #2
# This variable is associsted with the /api/rebuild/file endpoint
# The path to the file to be scanned by the engine
FILE_PATH="/home/shaban/Desktop/file.pdf"


## Endpoint #3
# This variable is associsted with the /api/rebuild/base64 endpoint
# The Base64 of the file to be scanned by the engine
BASE64_STRING="JVBERi0xLjUKJcOkw7zDtsOfCjIgMCBvYmoKPDwvTGVuZ3RoIDMgMCBSL0ZpbHRlci9GbGF0ZURlY29kZT4+CnN0cmVhbQp4nE2LPQvCUAxF9/yKzEKfN7HvCx4PWtHBrfDAQdy0boJd/PumdJE7nCQnF074Sx8Gw0ETBxGXo3DMG5cnXXf83j4sy4vGRj64xFF7K7cH78/CotzmW4FU7wsUh9ppQV+lwFebAmLtzKT1orZmDMYB42r/3LHe24VOjSaa+AdOeCGLCmVuZHN0cmVhbQplbmRvYmoKCjMgMCBvYmoKMTMwCmVuZG9iagoKNSAwIG9iago8PC9MZW5ndGggNiAwIFIvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aDEgMTA1NjQ+PgpzdHJlYW0KeJzlOX1wG9Wd77crWbZlW1pb/ooia5WNkzi2/LVOiNMk3tiW7MROLMd2kJIQS7ZkS2BLiiQnDRyHrxDIOOQINAUCuWvKAEMpd6xJrg0tB+YK7TE0hR5Mj0JzTae0nbmSIaWUdoDY93tPK8cOgU5v7r9beXd/39/v7UpOJSZCJI9MEp4ow+OB+LLykkJCyI8IgcLh/SlxY0/xlxC+QAj3HyPx0fGHvrPnQ0J0ZwgxnBkdOzhyu+uMjpC8MJ7vhUOBYH/9vU5CyvvRxtowEvpmDxoQvxvx5eHx1Jd1urU1iKuIN43FhgNvlRaaEH8X8aXjgS/HV+taOUKWIErEaGA89Ofj3w8iLhJiTMZjyVSQHJ4jpPI45ccToXj3Q0MvI47x8MeQBvihRx6CWRTneJ0+y5Cdk2vMI/8fD/1RUkw69RuJicTZddHBP0XKyQlC5t6j2JXrbPfcx/+XUWSnbw+Sx8kZcpT8jNygMdzEQyJkAikLjxfJT5BKDw/ZRZ4kU59j9ilyFvlpOT+5h2ZyzcNDHiCnyQ8XefGQcXILxvIv5GfQQF7BUYmRDyCb/B15Ga1+gLRt1zLFFeBlhIEjC6jvkIe5I2QrR+f4BOVwdZyZvEROwl60nMI8j85nvOEzRu8it+K1j4TJfoTZod/46dskZ+4PmNWtZCv5CtlMxhZoPAdf53Oxf/3k61jTFxmtLsM0dPI3ct/muMtfReReMopnADB37ii/+XMq9Fcf/ADJhyq+kuRci8s1EdPsx1zj3If8cpJLBuYuZWhzXXN/4AOzUd2gbql+o+7VL/KRda9uHLXJ3K9nb5kN6rfrH8duPUGI0rF7l8870N+3o9fTs31bd9fWLZ0dbld7W+tmpWXTxg1fWt+87rq1axrq62qdNatWrqhcLi1z2MssgtlUkG/Mzck2ZOl1PAekRlTB71L5SlFwBySXFOh01oiusnC7s8Yluf2qGBBVvOlWSJ2djCQFVNEvqivwFlhA9qsKSo5cJamkJZV5STCLG8gG6kIS1XPtkngWdvV6ET7aLvlE9SKDtzFYt4Ih+Yg4HKjBoqLRii7VvT885fJjjDBtzG2T2kK5zhoynWtE0IiQukqKT8OqTcAAbpVr/TRHsvOpW8zUFQiqnl6vq93qcPicNVvUAqmdsUgbM6lmtakGZlKM0NDJEXG6Zmbq7rNmMuSvzgtKwcAer8oHUHeKd01N3aUK1WqV1K5W3fxuGWYeUmukdpdaTa127Zj303XFJaj6SrMkTv2RYDrSxfcWUwIaJavS/EdCQZVrU2GH10EPqxtrPTXllkT3lH8qcHZuckgSzdLUdF7eVNyF5SYeL5o4O/fdI1bVfbdPNfvDsN6npe7e0aUW9e72qlylWwwHkIJ/LZJjndUhzMt4Po9NsCxYHKyww0HLcOSsQoYQUSd7vWlcJEPWZ4hSV+1TOT/lzGQ4xQOUM5nhzKv7JextV593StVVbglKLqz4kYA6OYTTdSNtjGRWCz6yOqSpQkFsrvMxWRGj2hKMiKp+BRYJtRYq4NxQlSkzQwo+St8uWtHBCqFQbJbQDLXjklx+7W9/uAwNiFjozur0IPR7VaUdASWgdcw1XV+HGgE/NizSzpqp1klx1SK1zneXhuWK9HmZiqamWtpU4h/WtNQ6F1tXomvK354OgdqSer3PEnnuwnSTaD0tkybia6fCJW04ZStcU97giGr3W4O47kZEr9WhKj7ssE/yhnx07LBCVResbDh8bFb6vV19UlfvLu86LZA0g5rTVbquMiN5rWkzOIBqdmW26OWsvA8FzUgQ3QhIrRvwqhoqs/E0Y8EZlQ5u6wbRC1aSkcYw1CrRFWrX5Ci+yKiejlNbZ8ZaFkXRTlun1eFzpA9nDYdsUXOMGtm0qJ0ZFm5TyMjG+WzrZCRayzI69KJXCkk+KSyqisdLc6PlYVXWisFqrvWqfxG2oFhYJuJAdgahxVTd1daFxVU7GD6Pdl7F3pJhi1PZUlffFDUuaQYJRr5FJXSElXWCle0FdEFLuPeKZlzSbEFPTSsKXczh9dSItCU4JfV5NzBp3E9utd5MfRWSLujqb3XW4NbWOi3B4d5pBQ737fI+a8b3wsP93mc44Nr8rb7p5cjzPiviQ4NROUqlRIqIFKGWdiCSzeStzyqETDKujhEYPnwWCKNlZ2hAhs9yaZo57WgFc6QQDjm6NEfJSOuQlp2mTTIaO6YJLZmSq1eylRwlj8vnrNNASc8g5bv4HpsD5HQe5IN1GrV2MPJZmJzOUaxpiUmUUNIRHh644npgl/d0Hj6dreyKjlrpgeNSFsZm42PFJQbpoPyNLzzl99HFRkqwNfgHKkibsE3SJgwkK0/NlUKtqlFqpfQWSm9J07Mo3YAjCiWA6pPYe48KdAJ2ex24JMUlr1inzBdpp3y4qUyZf+3E4Cz4VnNW34nvoIUwqHwgFJhMusJ8c16ewWDW8UWW/AKhwO8rFAQw4/M5z6AzgWnQlwuFH1rgXQu8aYGXLHDGAo9a4LgF7rBAygJBC/RboN0CTRZYbgGLBXQW+Gvlm79AYaG0jsnMWIBTLXDKAscsMGmBuAU8FlAsUG8B0QJmC1xgQlcJ9FjgBu3YN38M7tuXWHTsveGqY99VB2mRqwUiy3JZiywXNtfJ1aRWFgqhtFnAWzO9Njc31FcWO9ZcBzKU0jvv4IF3wLnZjgfhlefhnScvv3Lm0OVLd8GR38Aba9asser+/Em2Fe9w++ytuvDlCfryBaR/7j3uDf5lsor4lCaHwbIkH1tYtTrfwZeWVnh81lIzb/T4DHzJ5GqIrwb/avCsBnE1PL0aBldDz2rIhE9jJmU08EKCERYCDVPGv4Z6sGRJy1asXCOXlsiNa5rqoJZb07RWbiwtllaukJZlFVtKSit47o3pf3J/s97Z0PXlfzvhC+1p/Oax0YfrVq9J9A5s2/7VXS0SZN99zFb429vbH7+5yeZoH3b/zT32c+N1nvbm7Usaa9t20ldS0jn3Hr+Pf5FYSSUZV1qE7MpKnZiXV67j8RVxWe6yXl9ZsSAs9fhMgl3g8nhBINm5JQYd5lhMij0+Yp5cCYMrQVkJCNyAnSMtNC+WG7ZicO8NLEPan0xHtCwbS4oFbMVKTFZo2gQtsKYJszOBtGYtGAqg2CI3rr0OfvLQvROzs0WJ6d9vOfXg0Y6twb5l6x4Bcvudg/e0DzfyL/7tVy4fKnfuTUDZ3ls287qvBvbUTZyTZit0+r1R1V5Ge1aNb83l2LMGeEaZE/Kyli51kFWrnE5HHi83NtR6fA2mVY6lQp6z2unx2U3VxeVZWTk5lh2+HPNKfKnnK3f4ePN+GXbKsFaG5TKUyJAlw0cyvCvDmzL8QIZHZbhfhiEZwCNDuwz1TM4ig06G8KWM4BkZUjIoMjQxNvI+lOEdGWZkUJmNO2QIypqJtIw5I/a6DC/J8C0ZjjGxm2T4kgxixse6tINTMvhl6M/4sDDNd5nmcRkm0b1SvYBvZbrvsgA4lQnEmXv0apIhW1t4g4tW3LVW5aLVePW6HVwkhONBRwGXauYua4tXZrQrM0I/OCabQG4sKaXX8vTKFZqkZQWcIT09FMWpMWjwyjUOcHc9obgmbNtea790cHbg7lNLXK6WYuHobOuRgQHv7Udndx44AEW8v3p9U3N16+zvLt9f7nSWc96nsnPzdWs3Z9A+n+1yOQV5sdzJvr4Rx2w3r+IclRIHOaT0VuBOXVhalluau0wqLbQUenwWa77o8eWX2KwGa69PZzDzxOPjTYoEkxIQCZrrJbggwQzD/RIoC2Bcr/NFopXT1hGrTXo5aUuoObOEsDhFmC8rTUmxhZOWrSyxpUsE6Zrg9vHgTZDNrT665czL//nqvpGsR2eVA1zw1tsmtvtu/JQfKXdet7zm4/9+f/bjks6q2bK6ujJ++8z3HJdxleO6qcKkzfqj+G32ESWux++IWR4ffrPV83rMqvhNI7xkhDNGeNQIx41whxFSRggaYbkRLEbQGfEJwiSOGYGLG8FvBI8RFCPMGEE1wimGmo1AjHCJoSi3UGzR0NBpGrxq5tKDlB4ZurlfGYDYN2bLT50Ct5u2T8+V0eesB/c5N/aumCwlR5Vd5QCmJdnFpmJbRTn2yFRuL8fNrbw8r7CwxOMrNOfpe315JTMVoFbAqQo4VgGTFRCvAH8FeCqAVMAmvCkVUF8BYgWYK+ASk0OhzALYuzBQ1j3cBcu0/TCz2bMBL7ZUAN3qigsAt30Bd3pRKAbc5h1NK0C38bbRtcfr6x/b+c6rP34BIrMPhGNw3x74WeHUCU+hcZ299j3Qf/TB7MgOOPnEo6dP0Dndirn+FvtWRGxkUumx6IykvNysM1fYi8weX1GxKQ8zJgbc1A3mclTgSnt9XAmxQ4fHDood6u0g2gHxGTtMMkoa8DO61hUtyQXLmWTGNDOjLL3KLEkUmgrx6bViI2B36EIFC07mdcIKSeTe2PfA7G1vvzkWy/pHaE/N/nnWPnnHvl2+xOyn7l3wyz8BlDoOfVjm/PjZciece/57K7nfCuw53IJxP6l/hFhhrfJWYUkJb7WWFuXqbEtLrOVWj6+8mFiKLDimRSZDgcdnNIDVBjobfGiD79ngDhukbBC0QbVNo9/0rg3etMFLNjhjg+NMAtldC3S+xei7mY6F0V/N0NFWvw3aM/T1v2OGHrXBsQWummywnEkQG3CXbHDBBq/b4JQNJm0Qt4FiA9EGZhuoDDUzuUVb6+C13og+b8+lnCtvGESe3z1Km7W+FElrrlv0VmEDuVhKb6O/fOSRx762rbXBuay+penjj1+d1R3hvQ0rW1+/UHTuluL4Qyf7P/3Igc9PnDV8tnKF+m5ixJX1D8oIycvLEoTSEj6nz0d4MPN8sVKMOyPOnGAScOyKLaWgK8W9oRSOleLGUAr+UvCUglIKM6WglsIphoqlYC4FUgqXGAVFF0ouTl7bHPYu2BLIkjLzjzNLTUs4/VbFUi7hr2wWtyjOGkWpcSq5dNc4BNW6X6Rx5ZP189s/R96Z+xWc02/EPE1gUL5D8vOz8vLMQj5vKoA8voBX+JyHd/NFPxXgBwK4BVgrwLcFeFyAFQKUCJCmI+VRAY4L8BUBIC6AXwCPAO0CNAmwHN/wBSACNF8S4F0B3hRgRoAzGY1JQVPozyjoBEDJCwK8LsBLGckgE1AE4OoFEJnJ15khlZnQqjb42Wf14uEZvFJgSifpiv4IK9qyoKKVCysq8xKcczU1udyy7P7N25v27mno7GyQ3W7dP3/amX53xlEpPzH31HcfGzRt+COxp3///vf213985dfN2e6sclzV9MdxTiOhnsEx6yLXzwvBVT+J5mc141Pph8RCVXgb6eePkk6kVesIcXDNpApxD8JbuSdJC+LVeH+Hab5F3gIFnuF+ym/nz+hW607q/fo3s/YzD/mkUYuBI2ZSR/Yg8H3+B4Rn3AqIzsexcz4mQMmdGswRAxnRYB7fr8c1WIcyhzVYj14e1OAs/A74mAYbyM3kjAZnEwvUanAOKYBWDc6FKHg02EiWcs/P/5enlntbg/PJGj5bgwvIEn4jjV5Hf51+ir9eg4GIOl6DOVKgkzSYJ2t1DRqsQ5lRDdaTJbq7NDiLVOi+ocEG8qHuBQ3OJqv0pzU4hyzVv6PBudzP9X/SYCNZl/2GBueRPTlGDc4nN+ZkfBWQppyftEdGI6nIzaGgGAykAuJwLH4wERkNp8RVw1ViY31DvdgRi42OhcS2WCIeSwRSkVi0NrftarFGcQea6AykasQt0eHa7shQKC0r9oUSkZEdodGJsUBic3I4FA2GEqJTvFrianxnKJGkSGNtQ239FebVspGkGBBTiUAwNB5I3CTGRhbHISZCo5FkKpRAYiQqDtT21YqeQCoUTYmBaFDsn1fsGRmJDIcYcTiUSAVQOJYKY6Q3TiQiyWBkmHpL1s4nsKAafanQ/pC4LZBKhZKxaGsgib4wsv5INJasEQ+EI8Nh8UAgKQZDychoFJlDB8XFOiJyA5hLNBrbjyb3h2ow7pFEKBmOREfFJE1Z0xZT4UCKJj0eSiUiw4GxsYPYsvE4ag1hjw5EUmF0PB5KittDB8QdsfFA9MnadChYmxGsqRgZjydi+1mMzuRwIhSKorNAMDAUGYuk0Fo4kAgMY8WwbJHhJKsIFkKMB6JO10QiFg9hpNd3dF8RxADT1UzGxvajZyodDYWC1COGvT80hkroeCwWu4nmMxJLYKDBVNi5IPKRWDSFqjExEAxi4lit2PDEOO0TljmVCS4wnIghLz4WSKGV8WRtOJWKr6+rO3DgQG1Aa80wdqYWLdd9ES91MB7S+pGgVsbHurH9Udq6CdZfmkTflm6xJ471cWNwoiZQI2Yms6G2QXOBZYzEU8naZGSsNpYYretxd5N2EiGjeKbwvJmESJCIeAYQDyA0TGIkTg6SBJMKI1Ukq5BahfdGUk8a8BRJB0rFkD+G+iJpQziBWvQaYHZjJEpqSS7jfLG1RoR2aFF0Mu0ahLag/jBa6Ea9IeQutCuSPkaJ4DZLNUfJBMYRQMpmkkStEMoEmYRInHj+JRt/ib+TQcl5TiPG1YBn/TU1/5LdCFoSWaVTjEMjHWfR34S0GOp9UT1ElAux7iWRE2JYkFmltgdQoo9JeZgmrUSKeYsyqf5reOxBjyOoP8w6mZEcZrbpRKQtxxAOazW9EeudYBEEmV4mtyR6/mwHrj0bfSy6/cznNkaneJLxWhFPanmla9bPooghldbiAEZC/YYZHGD1DDJtOmNRTXMIp078Qj+iphvQ+hJlPvZrUVKdGq3eI+yaZH6j6ENk8aW7vNi3yOoUYFVPd3ocuSkmO4z0Mfwc1FbZOFYl7WtIW0cH2KoMaxmPM7si2Y73A2wqYqxvUccy1uMrVUnPzYg2pyLTjSMcY1lk6uhkvaGZhFikFAqwlT+EGmPMdzq2MJuOAOttSOt1imWQqVdQy5RGHWcUJ3GxuaDrPaTV9HrcJ7qvaTFdwYWzSXsyxuJNLrAdZdEG53NMV5tKjWme0hmPsf3opvn+jLB5S1c0yKw5P6fmI6w2Kc1rjEUUxE+64+nZiqHuBOtHej2lpzn1mcoFWH1jml6c7UopLZZxtj7CbALjZD2+WNZhdPRTy+Zw4aoZ1tZMrRZz3f9aj8YVZxVcuD4S87GMY4zd2uqPzq+6iQXrN9OJPtyDutl+Edfmx61VTrzKAl01V++ZDWzPXJxFehojiKdYPElWy1qWwyjye9BDN32HZsfcIQzpGsd0jmfzEIQIQBhGSRGxg59sh0EyAJvJRlDwriCvFe9tiNN7LWwkkyi3EembEN+A9C/h3mnHawuePXjeg6cOz7REPUrU4b1Ow52I16DGa3gFdlJqC1LpfSvinXjv0O5upLvw7tLwLYjjnfjBQH/QYNcXQKechguX4bXLIF6G2z4Bzycw+cGxD7jfX6qyP33phUtcz/uD7z/9Pl//Ppjeh2xy0XzRc9F/MX7x1MWsXNN7kEd+B8KvLqyz/2Lj+YH/2vjzAXIeMztff95zfvK8el5/HviBn/MldvOMOFM/E5+ZnHl95sLMpZnsyeePPc/963N1dtNz9uc4++me07ed5v1PgOkJ+xOc52H/w9yxk2A6aT9Zd5J/6ESt/URHhf2B+1faL9x/6X7u7NzM6fvzBfdz0APdZCPWcPtpfs7+9OZi2IZpmfBqx7MOzx48Y3jegyd+50FxO5510K2s4we/Bsb7rPdV33fLfUfu08fvnLzz2J385KFjh7in97+wn0t6quyxaLU92rHaXi6XDRhkfiAL3aB3ZctQ5Sq3f1CxD6LQ7l319l0dVfYiuXBAjwnrUNDE2/kWvoeP8ffwL/CG7B2eCnsvnhc8lzyc4snJc5t67D11PfzZuQtKqMuB1rbGt05u5be4q+ydHevspg57R13Hax2/6Hi/I2uwA76Of+6n3S+4ecVdVedW3BUO99JO60CJXDwggGnALJsGOMBGy2SgzjRn4kymQdNtJt5EWgg3WQJ6OAvHpvv7qqu7zhrmdnSp2Z7dKhxWK/voVendpWYdVsnArt3eaYC/9x06epS02rrUxj6v6rf5utQgAgoFJhEw26ZLSKsvmUxVswOqqxGewCupnqhG4t5kmkrm+aQ6CUncopJMCaqpQBoHvFZTHhKoHqD23iShF8qsTitR7aRmjimnLwwo2/s/UWg/LwplbmRzdHJlYW0KZW5kb2JqCgo2IDAgb2JqCjYxMzkKZW5kb2JqCgo3IDAgb2JqCjw8L1R5cGUvRm9udERlc2NyaXB0b3IvRm9udE5hbWUvQkFBQUFBK0xpYmVyYXRpb25TZXJpZgovRmxhZ3MgNAovRm9udEJCb3hbLTU0MyAtMzAzIDEyNzcgOTgxXS9JdGFsaWNBbmdsZSAwCi9Bc2NlbnQgODkxCi9EZXNjZW50IC0yMTYKL0NhcEhlaWdodCA5ODEKL1N0ZW1WIDgwCi9Gb250RmlsZTIgNSAwIFIKPj4KZW5kb2JqCgo4IDAgb2JqCjw8L0xlbmd0aCAyNzkvRmlsdGVyL0ZsYXRlRGVjb2RlPj4Kc3RyZWFtCnicXZFNboMwEIX3PoWX6SIyEJI0EkJKiSKx6I9KcwBjD9RSsS1jFty+9jhtpS5A3zDv2cMb1rSXVivP3pwRHXg6KC0dzGZxAmgPo9IkL6hUwt8rfIuJW8KCt1tnD1OrB1NVhL2H3uzdSjdnaXp4IOzVSXBKj3Rza7pQd4u1XzCB9jQjdU0lDOGcZ25f+AQMXdtWhrby6zZY/gQfqwVaYJ2nUYSRMFsuwHE9AqmyrKbV9VoT0PJfL/wCWvpBfHIXpHmQZlmZ14EL5CKLvEM+7iKXyAfU7BNfIh+SBvXH9L2J/Jh4H/mUuIx8TnyI/JT4FLlJ9+Y48H2yOHrM9icSKhbnQhy4AMwhJqA0/O7IGhtd+HwD8C6HNgplbmRzdHJlYW0KZW5kb2JqCgo5IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1RydWVUeXBlL0Jhc2VGb250L0JBQUFBQStMaWJlcmF0aW9uU2VyaWYKL0ZpcnN0Q2hhciAwCi9MYXN0Q2hhciAxMgovV2lkdGhzWzc3NyA3MjIgMjUwIDM4OSA0NDMgNzc3IDUwMCAyNzcgNDQzIDUwMCAzMzMgMjc3IDMzMyBdCi9Gb250RGVzY3JpcHRvciA3IDAgUgovVG9Vbmljb2RlIDggMCBSCj4+CmVuZG9iagoKMTAgMCBvYmoKPDwvRjEgOSAwIFIKPj4KZW5kb2JqCgoxMSAwIG9iago8PC9Gb250IDEwIDAgUgovUHJvY1NldFsvUERGL1RleHRdCj4+CmVuZG9iagoKMSAwIG9iago8PC9UeXBlL1BhZ2UvUGFyZW50IDQgMCBSL1Jlc291cmNlcyAxMSAwIFIvTWVkaWFCb3hbMCAwIDYxMiA3OTJdL0dyb3VwPDwvUy9UcmFuc3BhcmVuY3kvQ1MvRGV2aWNlUkdCL0kgdHJ1ZT4+L0NvbnRlbnRzIDIgMCBSPj4KZW5kb2JqCgo0IDAgb2JqCjw8L1R5cGUvUGFnZXMKL1Jlc291cmNlcyAxMSAwIFIKL01lZGlhQm94WyAwIDAgNjEyIDc5MiBdCi9LaWRzWyAxIDAgUiBdCi9Db3VudCAxPj4KZW5kb2JqCgoxMiAwIG9iago8PC9UeXBlL0NhdGFsb2cvUGFnZXMgNCAwIFIKL09wZW5BY3Rpb25bMSAwIFIgL1hZWiBudWxsIG51bGwgMF0KL0xhbmcoZW4tVVMpCj4+CmVuZG9iagoKMTMgMCBvYmoKPDwvQ3JlYXRvcjxGRUZGMDA1NzAwNzIwMDY5MDA3NDAwNjUwMDcyPgovUHJvZHVjZXI8RkVGRjAwNEMwMDY5MDA2MjAwNzIwMDY1MDA0RjAwNjYwMDY2MDA2OTAwNjMwMDY1MDAyMDAwMzYwMDJFMDAzND4KL0NyZWF0aW9uRGF0ZShEOjIwMjEwMTA4MTAyNDU1LTA4JzAwJyk+PgplbmRvYmoKCnhyZWYKMCAxNAowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDczMjMgMDAwMDAgbiAKMDAwMDAwMDAxOSAwMDAwMCBuIAowMDAwMDAwMjIwIDAwMDAwIG4gCjAwMDAwMDc0NjYgMDAwMDAgbiAKMDAwMDAwMDI0MCAwMDAwMCBuIAowMDAwMDA2NDY0IDAwMDAwIG4gCjAwMDAwMDY0ODUgMDAwMDAgbiAKMDAwMDAwNjY4MCAwMDAwMCBuIAowMDAwMDA3MDI4IDAwMDAwIG4gCjAwMDAwMDcyMzYgMDAwMDAgbiAKMDAwMDAwNzI2OCAwMDAwMCBuIAowMDAwMDA3NTY1IDAwMDAwIG4gCjAwMDAwMDc2NjIgMDAwMDAgbiAKdHJhaWxlcgo8PC9TaXplIDE0L1Jvb3QgMTIgMCBSCi9JbmZvIDEzIDAgUgovSUQgWyA8N0E4MDY2M0Q1RDQxRjlCMzJDNzhBMEJGNTczRkM0Q0Y+Cjw3QTgwNjYzRDVENDFGOUIzMkM3OEEwQkY1NzNGQzRDRj4gXQovRG9jQ2hlY2tzdW0gLzhFRTQ4QTJCNDM3QUI4RjU5NDgzQTFDMDlCQjE4NDY0Cj4+CnN0YXJ0eHJlZgo3ODM3CiUlRU9GCg=="


## Endpoint #4
# This variable is associsted with the /api/rebuild/zipfile endpoint
# The path to the zipfile to be scanned by the engine
ZIPFILE_PATH="/home/shaban/Desktop/file.zip"


## Endpoint #5
# This variable is associsted with the /api/rebuild/s3tozip endpoint
# The S3 presignedURL where zip file will be downloaded from.
presignedURL5='https://glasswalltask1.s3.eu-west-1.amazonaws.com/file.zip?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAWYWOGVNRJV2VKF6Y%2F20210122%2Feu-west-1%2Fs3%2Faws4_request&X-Amz-Date=20210122T230306Z&X-Amz-Expires=604800&X-Amz-SignedHeaders=host&X-Amz-Signature=71697f35e43f070e7d5b0a9c5eacfdcd4ca58bd0a95e6e37cf4f26ecf240caf9'


## Endpoint #6
# These variables are associsted with the /api/rebuild/s3tos3 endpoint
# The S3 presignedURL from where zip file will be downloaded.
presignedURL6='https://glasswalltask1.s3.eu-west-1.amazonaws.com/file.zip?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAWYWOGVNRJV2VKF6Y%2F20210122%2Feu-west-1%2Fs3%2Faws4_request&X-Amz-Date=20210122T230306Z&X-Amz-Expires=604800&X-Amz-SignedHeaders=host&X-Amz-Signature=71697f35e43f070e7d5b0a9c5eacfdcd4ca58bd0a95e6e37cf4f26ecf240caf9'
# The S3 targetpresignedURL which the protected file will be uploaded to
targetPresignedURL6='https://glasswalltask1.s3.amazonaws.com/test_public?AWSAccessKeyId=AKIAWYWOGVNRJV2VKF6Y&Signature=sYPPidBgzr7XbZqfa6VUN0xNEH0%3D&Expires=1611961585'


## Endpoint #7
# These variables are associsted with the /api/rebuild/ziptos3 endpoint
# The path to the zipfile to be scanned by the engine
ZIPFILE_PATH="/home/shaban/Desktop/file.zip"
# The S3 targetpresignedURL which the protected file will be uploaded to
targetPresignedURL7='https://glasswalltask1.s3.amazonaws.com/test_public?AWSAccessKeyId=AKIAWYWOGVNRJV2VKF6Y&Signature=sYPPidBgzr7XbZqfa6VUN0xNEH0%3D&Expires=1611961585'



### TESTING REQUESTS

## Endpoint #1
# The curl request and test for the /api/rebuild endpoint
rebuild=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/" -H "accept: */*" -H  "Content-Type: application/json" -d "{\"InputGetUrl\":\"$presignedURL1\",\"OutputPutUrl\":\"$targetPresignedURL1\",\"ContentManagementFlags\":{\"PdfContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"Javascript\":1,\"Acroform\":1,\"ActionsAll\":1},\"ExcelContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"DynamicDataExchange\":1,\"Macros\":1,\"ReviewComments\":1},\"PowerPointContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"Macros\":1,\"ReviewComments\":1},\"WordContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"DynamicDataExchange\":1,\"Macros\":1,\"ReviewComments\":1}}}")

if [ $rebuild = "200" ]
then
        echo "The endpoint '/api/rebuild' works fine."
else
        echo "The endpoint '/api/rebuild' does not work fine."
fi


## Endpoint #2
# The curl request and test for the /api/rebuild/file endpoint
rebuild_file=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/file" -H  "accept: application/octet-stream" -H  "Content-Type: multipart/form-data" -F "contentManagementFlagJson={
  "PdfContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Javascript": 1,
    "Acroform": 1,
    "ActionsAll": 1
  },
  "ExcelContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "PowerPointContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "WordContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  }
}" -F "file=@$FILE_PATH;type=application/pdf")

if [ $rebuild_file = "200" ]
then
	echo "The endpoint '/api/rebuild/file' works fine."
else
	echo "The endpoint '/api/rebuild/file' does not work fine."
fi


## Endpoint #3
# The curl request and test for the /api/rebuild/file endpoint
rebuild_base64=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/base64" -H  "accept: text/plain" -H  "Content-Type: application/json" -d "{\"Base64\":\"$BASE64_STRING\",\"ContentManagementFlags\":{\"PdfContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"Javascript\":1,\"Acroform\":1,\"ActionsAll\":1},\"ExcelContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"DynamicDataExchange\":1,\"Macros\":1,\"ReviewComments\":1},\"PowerPointContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"Macros\":1,\"ReviewComments\":1},\"WordContentManagement\":{\"Metadata\":1,\"InternalHyperlinks\":1,\"ExternalHyperlinks\":1,\"EmbeddedFiles\":1,\"EmbeddedImages\":1,\"DynamicDataExchange\":1,\"Macros\":1,\"ReviewComments\":1}}}")

if [ $rebuild_base64 = "200" ]
then
	echo "The endpoint '/api/rebuild/base64' works fine."
else
	echo "The endpoint '/api/rebuild/base64' does not work fine."
fi


## Endpoint #4
# The curl request and test for the /api/rebuild/zipfile endpoint
rebuild_zipfile=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/zipfile" -H  "accept: application/octet-stream" -H  "Content-Type: multipart/form-data" -F "contentManagementFlagJson={
  "PdfContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Javascript": 1,
    "Acroform": 1,
    "ActionsAll": 1
  },
  "ExcelContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "PowerPointContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "WordContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  }
}" -F "file=@$ZIPFILE_PATH;type=application/zip")

if [ $rebuild_zipfile = "200" ]
then
        echo "The endpoint '/api/rebuild/zipfile' works fine."
else
        echo "The endpoint '/api/rebuild/zipfile' does not work fine."
fi


## Endpoint #5
# The curl request and test for the /api/rebuild/s3tozip endpoint
rebuild_s3tozip=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/s3tozip" -H  "accept: application/octet-stream" -H  "Content-Type: multipart/form-data" -F "presignedURL=$presignedURL5" -F "contentManagementFlagJson={
  "PdfContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Javascript": 1,
    "Acroform": 1,
    "ActionsAll": 1
  },
  "ExcelContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "PowerPointContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "WordContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  }
}")

if [ $rebuild_s3tozip = "200" ]
then
        echo "The endpoint '/api/rebuild_s3tozip' works fine."
else
        echo "The endpoint '/api/rebuild_s3tozip' does not work fine."
fi


## Endpoint #6
# The curl request and test for the /api/rebuild/s3tos3 endpoint
rebuild_s3tos3=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/s3tos3" -H  "accept: application/json" -H  "Content-Type: multipart/form-data" -F "sourcePresignedURL=$presignedURL6" -F "targetPresignedURL=$targetPresignedURL6" -F "contentManagementFlagJson={
  "PdfContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Javascript": 1,
    "Acroform": 1,
    "ActionsAll": 1
  },
  "ExcelContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "PowerPointContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "WordContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  }
}")

if [ $rebuild_s3tos3 = "200" ]
then
        echo "The endpoint '/api/rebuild_s3tos3' works fine."
else
        echo "The endpoint '/api/rebuild_s3tos3' does not work fine."
fi


## Endpoint #7
# The curl request and test for the /api/rebuild/ziptos3 endpoint
rebuild_ziptos3=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$URL:$PORT/api/rebuild/ziptos3" -H  "accept: application/octet-stream" -H  "Content-Type: multipart/form-data" -F "targetPresignedURL=$targetPresignedURL7" -F "contentManagementFlagJson={
  "PdfContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Javascript": 1,
    "Acroform": 1,
    "ActionsAll": 1
  },
  "ExcelContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "PowerPointContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "Macros": 1,
    "ReviewComments": 1
  },
  "WordContentManagement": {
    "Metadata": 1,
    "InternalHyperlinks": 1,
    "ExternalHyperlinks": 1,
    "EmbeddedFiles": 1,
    "EmbeddedImages": 1,
    "DynamicDataExchange": 1,
    "Macros": 1,
    "ReviewComments": 1
  }
}" -F "file=@$ZIPFILE_PATH;type=application/zip")

if [ $rebuild_ziptos3 = "200" ]
then
        echo "The endpoint '/api/rebuild_ziptos3' works fine."
else
        echo "The endpoint '/api/rebuild_ziptos3' does not work fine."
fi
